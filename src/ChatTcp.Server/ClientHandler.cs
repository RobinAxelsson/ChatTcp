using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace ChatTcp.Server;

internal class ClientHandler : IDisposable
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _networkStream;
    private readonly StreamWriter _streamWriter;
    private readonly StreamReader _streamReader;

    public string Username { get; set; } = "Unknown";

    public ClientHandler(TcpClient tcpClient)
    {
        _tcpClient = tcpClient;
        Username = _tcpClient.Client.RemoteEndPoint?.ToString() ?? "Unknown";
        _networkStream = _tcpClient.GetStream();
        _streamWriter = new StreamWriter(_networkStream, Encoding.UTF8) { AutoFlush = true };
        _streamReader = new StreamReader(_networkStream, Encoding.UTF8);
    }

    public async Task SendChatMessageToClient(ChatMessageDto chatMessage)
    {
        var json = JsonSerializer.Serialize(chatMessage);
        await _streamWriter.WriteLineAsync(json);
        await _streamWriter.FlushAsync();
    }

    public async void Listen(Func<ChatMessageDto, ClientHandler, CancellationToken, Task> onReceivedMessage, CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000);

            var networkString = await _streamReader.ReadLineAsync();

            if (networkString == null)
            {
                throw new ChatTcpServerException(nameof(networkString) + " was null when reading stream");
            }

            var chatMessage = JsonSerializer.Deserialize<ChatMessageDto>(networkString);

            if (chatMessage == null)
            {
                throw new ChatTcpServerException("Failed deserialize " + nameof(chatMessage));
            }

            if(string.IsNullOrEmpty(chatMessage.Message) || string.IsNullOrEmpty(chatMessage.Sender))
            {
                throw new ChatTcpServerException("Invalid chatmessage from stream, content '" + networkString + "'");
            }

            await onReceivedMessage(chatMessage, this, ct);
        }
    }

    public void Dispose()
    {
        _streamReader.Dispose();
        _streamWriter.Dispose();
        _networkStream.Dispose();
        _tcpClient.Dispose();
    }
}
