using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Reactive.Subjects;
using ChatTcp.Kernel;

namespace ChatTcp.Cli.Shell;

internal sealed class NetworkManager : IDisposable
{
    private readonly ConcurrentQueue<WirePacketDto> _outboundPacketQueue = new();
    private readonly Subject<WirePacketDto> _inboundPackets = new();

    private readonly TcpClient _tcpClient = new();
    private NetworkStream? _networkStream;

    public Action<WirePacketDto>? OnPacketReceivedFromServer { get; set; }


    public async Task StartAsync(CancellationTokenSource cts, string host = "localhost", int port = 8888)
    {
        await _tcpClient.ConnectAsync(host, port);
        _networkStream = _tcpClient.GetStream();
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
        }
    }



    public void SendChatMessageToServer(ChatMessageDto chatMessage)
    {
        _outboundPacketQueue.Enqueue(chatMessage);
    }

    internal async Task<JoinChatResponseDto?> SendJoinChatRequest(JoinChatDto joinChatDto, CancellationToken ct)
    {
        _outboundPacketQueue.Enqueue(joinChatDto);
        JoinChatResponseDto? joinChatResponseDto = null;
        _inboundPackets.Subscribe(x =>
        {
            if (x != null && x.Id == joinChatDto.Id)
            {
                joinChatResponseDto = (JoinChatResponseDto)x;
            }
        });

        while (!ct.IsCancellationRequested)
        {
            if(joinChatResponseDto != null)
            {
                return joinChatResponseDto;
            }
            await Task.Delay(100, ct);
        }

        return null;
    }

    private async Task SendMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_outboundPacketQueue.TryDequeue(out var message))
            {
                if(_networkStream == null)
                {
                    throw new ShellException(nameof(_networkStream) + " is null");
                }

                switch (message)
                {
                    case JoinChatDto joinChatDto:
                        await PacketStream.WritePacketAsync(joinChatDto, _networkStream, ct);
                        break;
                    case ChatMessageDto chatMessageDto:
                        await PacketStream.WritePacketAsync(chatMessageDto, _networkStream, ct);
                        break;
                    case JoinChatResponseDto joinChatResponseDto:
                        await PacketStream.WritePacketAsync(joinChatResponseDto, _networkStream, ct);
                        break;
                    default:
                        throw new ShellException("Not implimented");
                }
            }
            await Task.Delay(500);
        }

        Console.WriteLine(nameof(SendMessages) + " exited gracefully");
    }

    private async Task ReceiveMessages(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_networkStream == null)
            {
                throw new ShellException(nameof(_networkStream) + " is null");
            }

            var message = await PacketStream.ReadPacketAsync(_networkStream, ct);
            _inboundPackets.OnNext(message);
            OnPacketReceivedFromServer?.Invoke(message);
        }

        Console.WriteLine(nameof(ReceiveMessages) + " exited gracefully");
    }

    public void Dispose()
    {
        _networkStream?.Dispose();
        _tcpClient.Dispose();
    }
}

