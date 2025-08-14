//Instructions chatGpt design reasoning:
//You are a .NET senior developer
//You value simple design solutions over complex
//Keep an open mind and ignore previous chats for now
//Read the full file and reason about the design, thread safety and readability
//Can you simplify, is it any wrong abstractions, can some things be done differently
//then read it again part by part and write comments as you go
//then assume you could be wrong and iterate again
//Only comment inside the code
//Do the instructions


using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;

namespace ChatTcp.Cli;

// High-level pass: Design is clear (accept → per-conn send/receive). Main risks: StringBuilder thread-safety,
// duplicated connection bookkeeping across several collections, and Channel<object> (type-unsafe).
// Consider a single ConnectionContext (Connection + Channel<ChatMessageDto> + ReceiveTask) to prefer
// "collection of objects" over "objects spread across collections". This will also reduce coordination bugs.
internal sealed class NetworkSystemWithGptComments : IChatMessageSubscriber
{
    //TODO implement lock on StringBuilder
    private readonly StringBuilder _memory = new();

    // Listeners lifecycle looks owned here. Ensure StartAsync cancellation disposes/stops them in finally.
    private readonly List<Listener> _listeners;

    // Multiple registries track the same connection (list + dictionaries). This invites skew/races.
    // Prefer a single concurrent collection of ConnectionContext objects to hold all per-connection state.
    private readonly List<Connection> _connections = new();

    // Using Channel<object> loses type safety and forces runtime checks in the send loop.
    // If only ChatMessageDto is outbound, make this Channel<ChatMessageDto>.
    private readonly ConcurrentDictionary<Connection, Channel<object>> _outbound = new();

    // Stored but never awaited elsewhere. If not observed for coordination or shutdown, remove it.
    private readonly ConcurrentDictionary<Connection, Task> _receiveLoops = new();

    // Accept loops are awaited in StartAsync; this is fine. Be mindful of listener disposal on exit.
    private readonly List<Task> _acceptLoops = new();

    // Consider injecting IConsoleWriter for testability; default to singleton to keep simplicity.
    private readonly ConsoleWriter _consoleWriter = ConsoleWriter.Instance;

    // Property exposes the list reference (readonly field). Fine since list is private and only read here.
    internal IReadOnlyList<Listener> Listeners => _listeners;

    // Returns a snapshot under lock. Reasonable to protect readers from concurrent mutation.
    internal IReadOnlyList<Connection> Connections
    {
        get
        {
            lock (_connections) { return _connections.ToList(); }
        }
    }

    public NetworkSystemWithGptComments(List<Listener> listeners)
    {
        if (listeners is null) throw new InvalidStateException("Listeners is null.");
        if (listeners.Count == 0) throw new InvalidStateException("Listeners cannot be empty.");
        _listeners = listeners;
    }

    public void OnChatMessageCreated(ChatMessageEntity message)
    {
        if (message is null) throw new InvalidStateException("Message is null.");

        WriteLine($"[{message.ChatId}] {message.Message}");
    }

    public async Task StartAsync(CancellationToken token)
    {
        //TODO try/finally to stop/Dispose listeners when StartAsync completes/cancels.
        var ls = _listeners;
        for (var i = 0; i < ls.Count; i++)
        {
            var listener = ls[i];
            if (listener is null) throw new InvalidStateException("Listener is null.");

            if (listener.State == ListenerState.Created)
            {
                WriteLine($"{listener} created");
                listener.Start();
                WriteLine($"{listener} started");
            }
            else if (listener.State != ListenerState.Listening)
            {
                throw new InvalidStateException("State not implemented: " + listener.State);
            }

            _acceptLoops.Add(RunAcceptLoopAsync(listener, token));
        }

        await Task.WhenAll(_acceptLoops).ConfigureAwait(false);
    }

    private async Task RunAcceptLoopAsync(Listener listener, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var tcpClient = await listener.Socket.AcceptTcpClientAsync(ct).ConfigureAwait(false);
                var conn = new Connection(tcpClient);
                lock (_connections) { _connections.Add(conn); }
                WriteLine($"{conn} created");

                // SingleReader = true matches single send loop; good. object reduces clarity; use ChatMessageDto.
                var channel = Channel.CreateUnbounded<object>(
                    new UnboundedChannelOptions { SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = true });

                if (!_outbound.TryAdd(conn, channel)) throw new InvalidStateException("Failed to register connection channel.");

                // Fire-and-forget send loop; relies on CloseConnectionAsync for cleanup.
                _ = RunSendLoopAsync(conn, channel.Reader, ct);

                
                // Store receive loop task but never consumed. Either await later or remove storage.
                _receiveLoops[conn] = RunReceiveLoopAsync(conn, () => { }, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // Log + small backoff is good to avoid hot loop on repeated failures.
                WriteLine($"{listener} accept failed: {ex}");
                try { await Task.Delay(250, ct).ConfigureAwait(false); } catch { }
            }
        }
    }

    private async Task RunSendLoopAsync(Connection conn, ChannelReader<object> reader, CancellationToken ct)
    {
        try
        {
            // WaitToReadAsync + TryRead loop is efficient. With typed reader, inner type check disappears.
            while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                object? msg;
                while (reader.TryRead(out msg))
                {
                    try
                    {
                        if (msg is ChatMessageDto chat)
                        {
                            await PacketStream.WritePacketAsync(chat, conn.NetworkStream, ct).ConfigureAwait(false);
                            WriteLine($"{conn} sent {chat}");
                        }
                        else
                        {
                            // With Channel<ChatMessageDto>, this path goes away, improving clarity.
                            WriteLine($"{conn} unknown outbound message type: {msg?.GetType().Name}");
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        // Escalate to terminate send loop → connection shutdown in finally.
                        WriteLine($"{conn} send faulted: {ex}");
                        throw;
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            WriteLine($"{conn} send loop terminating: {ex}");
        }
        finally
        {
            await CloseConnectionAsync(conn).ConfigureAwait(false);
        }
    }

    private async Task RunReceiveLoopAsync(Connection conn, Action onClosed, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var packetDto = await PacketStream.ReadPacketAsync(conn.NetworkStream, ct).ConfigureAwait(false);
                WriteLine($"{conn} received {packetDto.GetType().Name} {packetDto.Id}");

                // Flat branching (low nesting). Good.
                if (packetDto is ChatMessageDto chat)
                {
                    await BroadcastAsync(conn, chat, ct).ConfigureAwait(false);
                    continue;
                }

                if (packetDto is JoinChatDto joinChat)
                {
                    // Consider validating/auth’ing join here or routing to a join handler.
                    WriteLine($"{joinChat.Id} wants to join chat");
                    continue;
                }

                // Policy: invalid packet closes the connection. Reasonable for protocol safety.
                WriteLine($"{conn} invalid packet type received");
                return;
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            WriteLine($"{conn} receive loop faulted: {ex}");
        }
        finally
        {
            try { onClosed(); } catch { }
            await CloseConnectionAsync(conn).ConfigureAwait(false);
        }
    }

    private async Task BroadcastAsync(Connection from, ChatMessageDto chat, CancellationToken ct)
    {
        // Snapshot avoids concurrent enumeration hazards and keeps send fairness decent.
        var snapshot = _outbound.ToArray();
        for (var i = 0; i < snapshot.Length; i++)
        {
            var conn = snapshot[i].Key;
            if (conn == from) { continue; }

            var writer = snapshot[i].Value.Writer;

            // TryWrite first for the fast path; fallback to async write for backpressure. Good pattern.
            if (!writer.TryWrite(chat))
            {
                try { await writer.WriteAsync(chat, ct).ConfigureAwait(false); }
                catch (ChannelClosedException) { }
            }

            WriteLine($"{conn} queued send {chat}");
        }
    }

    private async Task CloseConnectionAsync(Connection conn)
    {
        // Idempotent enough: TryRemove + TryComplete; safe if called from both loops.
        if (_outbound.TryRemove(conn, out var ch))
        {
            try { ch.Writer.TryComplete(); } catch { }
        }

        lock (_connections)
        {
            var removed = _connections.Remove(conn);
            if (!removed) { WriteLine($"{conn} was not in connections list"); }
        }

        WriteLine(conn + " removed");

        try { conn.Dispose(); } catch { }

        await Task.CompletedTask;
    }

    private void WriteLine(string text)
    {
        // Risk: unsynchronized StringBuilder updates. Add lock if you need memory log to be consistent.
        _memory.Append(text);
        _consoleWriter.WriteText(text);
    }
}

// --- Re-check / iteration assuming I could be wrong ---
// If outbound types will grow, Channel<object> might be intentional. Still, a small sealed IMessage marker
// with specific records is safer than raw object. If StringBuilder contention is rare (e.g., dev CLI),
// races may be acceptable for logs; but correctness of _memory should then not be relied on functionally.
// If external code awaits _receiveLoops elsewhere (not in this file), keeping the dictionary is justified;
// otherwise, it's confusing state and should be dropped or awaited on shutdown for graceful completion.
// If listeners are managed elsewhere, listener disposal in StartAsync may be outside this class’s remit.
// If backpressure needs stronger guarantees, consider bounded channels per connection to avoid unbounded growth.
