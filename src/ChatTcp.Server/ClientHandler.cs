using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ChatTcp.Kernel;

namespace ChatTcp.Server;

internal class ClientHandler : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _networkStream;

    public string Username { get; set; } = "Unknown";

    public ClientHandler(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        Username = _tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        _networkStream = _tcpClient.GetStream();
    }

    public async Task SendChatMessageToClient(ChatMessageDto chatMessage, CancellationToken ct)
    {
        await PacketStream.WritePacketAsync(chatMessage, _networkStream, ct);
    }

    public async void Listen(Func<ChatMessageDto, ClientHandler, CancellationToken, Task> onReceivedMessage, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var chatMessage = await PacketStream.ReadPacketAsync(_networkStream, ct);
            await onReceivedMessage((ChatMessageDto)chatMessage, this, ct);
        }
    }

    public void Dispose()
    {
        _networkStream.Dispose();
        _tcpClient.Dispose();
    }
}
