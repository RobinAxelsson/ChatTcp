using System.Collections.Concurrent;
using System.Text;
using System.Threading.Channels;
using ChatTcp.Kernel;
using ChatTcp.Kernel.Resources;

namespace ChatTcp.Cli;

internal class NetworkSystem
{
    private StringBuilder _memory = new StringBuilder();
    private List<Listener> _listeners { get; }
    private List<Connection> _connections { get; } = new();
    private readonly ConcurrentDictionary<Connection, Channel<object>> _outbound = new();
    private readonly ConcurrentDictionary<Connection, Task> _receiveLoops = new();
    private readonly ConsoleWriter _consoleWriter = ConsoleWriter.Instance;

    // Top-level tasks
    private readonly List<Task> _acceptLoops = new();

    public NetworkSystem(List<Listener> listeners)
    {
        _listeners = listeners;
    }

    private void WriteLine(string text)
    {
        _memory.Append(text);
        _consoleWriter.WriteText(text);
    }

    /// <summary>
    /// Starts listeners and runs accept/receive/send loops until <paramref name="token"/> is canceled.
    /// Completes when all loops have exited.
    /// </summary>
    public async Task StartAsync(CancellationToken token)
    {
        foreach (var listener in _listeners)
        {
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


        // Wait here until canceled (or a fatal error stops all loops)
        await Task.WhenAll(_acceptLoops).ConfigureAwait(false);
    }

    private async Task RunAcceptLoopAsync(Listener listener, CancellationToken ct)
    {
        // If you want to cap concurrent connections per listener, uncomment this gate
        // var gate = new SemaphoreSlim(listener.ReceiversMax);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // await gate.WaitAsync(ct); // if using the gate
                var tcpClient = await listener.Socket.AcceptTcpClientAsync(ct).ConfigureAwait(false);
                var conn = new Connection(tcpClient);

                lock (_connections) _connections.Add(conn);
                WriteLine($"{conn} created");

                // Create per-connection outbound queue and start its send loop
                var channel = Channel.CreateUnbounded<object>(
                    new UnboundedChannelOptions { SingleReader = true, SingleWriter = false, AllowSynchronousContinuations = true });
                _outbound[conn] = channel;
                _ = RunSendLoopAsync(conn, channel.Reader, ct);

                await channel.Writer.WriteAsync(
                    new ChatMessageDto(listener.ToString(), "Established connection"), ct).ConfigureAwait(false);

                // Start the receive loop
                _receiveLoops[conn] = RunReceiveLoopAsync(conn, /*onClosed:*/ () =>
                {
                    // gate.Release(); // if using the gate
                }, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                WriteLine($"{listener} accept failed: {ex}");
                // brief backoff to avoid a tight error loop
                try { await Task.Delay(250, ct).ConfigureAwait(false); } catch { }
            }
        }
    }

    private async Task RunSendLoopAsync(Connection conn, ChannelReader<object> reader, CancellationToken ct)
    {
        try
        {
            while (await reader.WaitToReadAsync(ct).ConfigureAwait(false))
            {
                while (reader.TryRead(out var msg))
                {
                    try
                    {
                        switch (msg)
                        {
                            case ChatMessageDto chat:
                                await PacketStream.WritePacketAsync(chat, conn.NetworkStream, ct).ConfigureAwait(false);
                                WriteLine($"{conn} sent {chat}");
                                break;

                            default:
                                WriteLine($"{conn} unknown outbound message type: {msg?.GetType().Name}");
                                break;
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        WriteLine($"{conn} send faulted: {ex}");
                        throw; // fail the loop -> connection will be closed
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

                switch (packetDto)
                {
                    case ChatMessageDto chat:
                        await BroadcastAsync(conn, chat, ct).ConfigureAwait(false);
                        break;

                    case JoinChatDto joinChat:
                        WriteLine($"{joinChat.Id} wants to join chat");
                        break;

                    default:
                        WriteLine($"{conn} invalid packet type received");
                        // optional: close connection on invalid input
                        return;
                }
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
        foreach (var kvp in _outbound)
        {
            var conn = kvp.Key;
            if (conn == from) continue;

            var writer = kvp.Value.Writer;
            if (!writer.TryWrite(chat))
            {
                try { await writer.WriteAsync(chat, ct).ConfigureAwait(false); }
                catch (ChannelClosedException) { /* connection is closing */ }
            }
            WriteLine($"{conn} queued send {chat}");
        }
    }

    private async Task CloseConnectionAsync(Connection conn)
    {
        if (_outbound.TryRemove(conn, out var ch))
        {
            try { ch.Writer.TryComplete(); } catch { }
        }

        lock (_connections) _connections.Remove(conn);

        WriteLine(conn + " removed");

        try { conn.Dispose(); } catch { }

        // await anything connection-specific if needed
        await Task.CompletedTask;
    }
}
