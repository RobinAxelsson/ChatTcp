
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CliChat.Cli;

internal class MessageClient
{
    public static void ConnectToServer()
    {
        using var client = new TcpClient();
        client.Connect(IPAddress.Loopback, 8888);
        var endpoint = $"{IPAddress.Loopback}:{8888}";

        Console.WriteLine($"Client connected to server: {IPAddress.Loopback}:{8888}");
        using var networkStream = client.GetStream();
        using var streamWriter = new StreamWriter(networkStream, Encoding.UTF8) { AutoFlush = true };
        using var streamReader = new StreamReader(networkStream, Encoding.UTF8);

        var readTask = ReadLoop(streamReader, endpoint);
        var writeTask = WriteLoop(streamWriter);

        Task.WaitAll(readTask, writeTask);
    }

    private static async Task WriteLoop(StreamWriter streamWriter)
    {
        while (true)
        {
            var message = Console.ReadLine();
            await streamWriter.WriteLineAsync(message);
            Thread.Sleep(1000);
        }
    }

    private static async Task ReadLoop(StreamReader streamReader, string remoteEndpoint)
    {
        string? message;

        while (true)
        {
            Thread.Sleep(1000);

            message = await streamReader.ReadLineAsync();

            if (message == null) continue;

            Console.WriteLine($"{remoteEndpoint}:{message}");
        }
    }
}