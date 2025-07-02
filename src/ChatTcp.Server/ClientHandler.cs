using System.Net.Sockets;
using ChatTcp.Kernel;

namespace ChatTcp.Server;

internal class ClientHandler : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _networkStream;

    public string? Username { get; set; }
    public string? RemoteEndPoint { get; private set; }

    public ClientHandler(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        _networkStream = _tcpClient.GetStream();
        RemoteEndPoint = _tcpClient.Client.RemoteEndPoint?.ToString();
    }

    public async Task SendChatMessageToClient(ChatMessageDto chatMessage, CancellationToken ct)
    {
        await PacketStream.WritePacketAsync(chatMessage, _networkStream, ct);
    }

    public async Task Listen(Func<ChatMessageDto, ClientHandler, CancellationToken, Task> onReceivedMessage, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var chatMessage = await PacketStream.ReadPacketAsync(_networkStream, ct);
            if(chatMessage is ChatMessageDto && Username == null)
            {
                Username = ((ChatMessageDto)chatMessage).Sender;
            }

            await onReceivedMessage((ChatMessageDto)chatMessage, this, ct);
        }
    }

    public void Dispose()
    {
        _networkStream.Dispose();
        _tcpClient.Dispose();
    }
}
