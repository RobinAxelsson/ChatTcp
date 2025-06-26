using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal sealed class NetworkManager : IDisposable
{
    private readonly Subject<AppEvent> _events = new();
    private readonly ConcurrentQueue<ChatMessageDto> _outboundChatMessageQueue = new();
    private readonly TcpClient _tcpClient = new();
    private NetworkStream? _networkStream;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public IObservable<AppEvent> Events => _events.AsObservable();

    public void QueueOutboundChatMessage(ChatMessageDto chatMessage)
    {
        _outboundChatMessageQueue.Enqueue(chatMessage);
    }

    public async Task StartAsync(CancellationTokenSource cts, string host = "localhost", int port = 8888)
    {
        await _tcpClient.ConnectAsync(host, port);
        _networkStream = _tcpClient.GetStream();
        _reader = new StreamReader(_networkStream, Encoding.UTF8);
        _writer = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };

        //TODO: TDD your way with these streams
        //var firstByte = _networkStream.ReadByte();
        //var version = (firstByte & 0xf0) >> 4; //Which endian?
        //var frameType = firstByte & 0x04;
        //var payloadLength = _networkStream.ReadByte();
        //var payload = new byte[255];
        //var messageBytes = _networkStream.Read(payload, 0, payloadLength);

        //var sMessage = Encoding.UTF8.GetString(payload, 0, payloadLength);

        _events.OnNext(new ConnectedEvent());

        var receiveTask = ReceiveMessages(cts.Token);
        var sendTask = SendMessages(cts.Token);

        try
        {
            //If any task throws we want to fail fast
            await await Task.WhenAny(receiveTask, sendTask);
        }
        catch
        {
            cts.Cancel();
            throw;
        }
        finally
        {
            await Task.WhenAll(receiveTask, sendTask);
            _events.OnNext(new DisconnectedEvent());
            _events.OnCompleted();
        }
    }

    private async Task SendMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_outboundChatMessageQueue.TryDequeue(out var message))
            {
                if (_writer == null)
                    throw new InvalidOperationException("Not connected");

                var json = JsonSerializer.Serialize(message);

                await _writer.WriteLineAsync(json);
            }
            await Task.Delay(500);
        }

        Console.WriteLine(nameof(SendMessages) + " exited gracefully");
    }

    private async Task ReceiveMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_reader == null)
            {
                throw new ShellException("reader is null");
            }

            var networkString = await _reader.ReadLineAsync(ct);

            if(networkString == null)
            {
                throw new ShellException("Networkmanager got null from server ReadLineAsync");
            }
            //var message = ChatMessageDto.FromOtherUser("", networkString);
            var message = JsonSerializer.Deserialize<ChatMessageDto>(networkString);

            if (message == null)
                throw new ShellException("Failed deserialize text string: " + networkString);

            _events.OnNext(new NetworkReceiveEvent(message));
        }


        Console.WriteLine(nameof(ReceiveMessages) + " exited gracefully");
    }

    public void Dispose()
    {
        _reader?.Dispose();
        _writer?.Dispose();
        _networkStream?.Dispose();
        _tcpClient.Dispose();

        _events.OnCompleted();
        _events.Dispose();
    }
}

