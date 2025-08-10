using System.Net;
using System.Net.Sockets;

namespace ChatTcp.Kernel;

public enum ServerState
{
    Off,
    Starting,
    Start,
    Running,
    Exitting,
    Exited
}
public static class ChatTcpManager
{
    public static TcpListener? TcpServer = null;
    public static List<(Entity Id, Task<TcpClient> Task)> TcpClientsAwaiters = new();
    public static List<(Entity Id, TcpClient TcpClient)> TcpClients = new();
    public static List<(Entity Id, NetworkStream NetworkStream)> NetworkStreams = new();
    public static List<(Entity Id, Task<WirePacketDto> Task)> IncomingPacketAwaiters = new();
    public static List<(Entity Id, Task Task)> SendMessageAwaiters = new();

    public static ServerState ServerState = ServerState.Start;
    public static void Run()
    {
        var cts = new CancellationTokenSource();

        while (!cts.IsCancellationRequested)
        {
            if (ServerState == ServerState.Start && TcpServer == null)
            {
                Console.WriteLine("Starting server");
                ServerState = ServerState.Starting;
                TcpServer = new TcpListener(IPAddress.Loopback, 8888);
                TcpServer.Start();
                ServerState = ServerState.Running;
                Console.WriteLine("Server running");
            }
            if (TcpClientsAwaiters.Count() < 3)
            {
                var tcpClient = TcpServer!.AcceptTcpClientAsync();
                TcpClientsAwaiters.Add((Entity.New, tcpClient));
                Console.WriteLine("Added TcpClient awaiter");
            }

            for (int i = TcpClientsAwaiters.Count - 1; i >= 0; i--)
            {
                var client = TcpClientsAwaiters[i];
                if (client.Task.IsCompletedSuccessfully)
                {
                    TcpClientsAwaiters.RemoveAt(i);
                    TcpClients.Add((client.Id, client.Task.Result));
                    Console.WriteLine("Client connected: " + client.Task.Result.Client.RemoteEndPoint);
                }
                if (client.Task.IsFaulted || client.Task.IsCanceled)
                {
                    TcpClientsAwaiters.RemoveAt(i);
                    Console.WriteLine(client.Task.Exception);
                }
            }

            for (int i = TcpClients.Count - 1; i >= 0; i--)
            {
                var client = TcpClients[i];
                if (NetworkStreams.FirstOrDefault(x => x.Id == client.Id) == default)
                {
                    var networkStream = client.TcpClient.GetStream();
                    NetworkStreams.Add((client.Id, networkStream));
                    Console.WriteLine("Established networkStream");
                    SendMessageAwaiters.Add((client.Id, PacketStream.WritePacketAsync(new ChatMessageDto("Server", $"Established connection to {TcpServer!.Server.RemoteEndPoint} {TcpServer.LocalEndpoint}"), networkStream, cts.Token)));
                }
            }

            for (int i = NetworkStreams.Count - 1; i >= 0; i--)
            {
                var stream = NetworkStreams[i];
                if (IncomingPacketAwaiters.FirstOrDefault(x => x.Id == stream.Id) == default)
                {
                    IncomingPacketAwaiters.Add((stream.Id, PacketStream.ReadPacketAsync(stream.NetworkStream, cts.Token)));
                    Console.WriteLine("Added new packetDto awaiter");
                }
            }

            for (int i = IncomingPacketAwaiters.Count - 1; i >= 0; i--)
            {
                var packetAwaiter = IncomingPacketAwaiters[i];
                if (packetAwaiter.Task.IsCompletedSuccessfully)
                {
                    Console.WriteLine("Message received");
                    IncomingPacketAwaiters.RemoveAt(i);
                    var packetDto = packetAwaiter.Task.Result;

                    switch (packetDto)
                    {
                        case ChatMessageDto chat:
                            Console.WriteLine($"Relaying {chat.Id}: {chat.Message}");

                            for (int sI = NetworkStreams.Count - 1; sI >= 0; sI--)
                            {
                                var networkStream = NetworkStreams[sI];
                                if(networkStream.Id == packetAwaiter.Id)
                                    continue;

                                SendMessageAwaiters.Add((networkStream.Id, PacketStream.WritePacketAsync(chat, networkStream.NetworkStream, cts.Token)));
                            }

                            break;
                        case JoinChatDto joinChat:
                            Console.WriteLine($"{joinChat.Id} wants to join chat");
                            break;
                        default:
                            throw new ChatTcpKernelException("invailid type");
                    }

                    if (NetworkStreams.FirstOrDefault(x => x.Id == packetAwaiter.Id) == default)
                    {
                        throw new ChatTcpKernelException($"{packetAwaiter.Id} should have networkstream networkStream");
                    }

                    var stream = NetworkStreams.FirstOrDefault(x => x.Id == packetAwaiter.Id).NetworkStream;
                    var sendMessageAwaiter = PacketStream.WritePacketAsync(new ChatMessageDto("Server", "Received packet"), stream, cts.Token);
                    SendMessageAwaiters.Add((packetAwaiter.Id, sendMessageAwaiter));
                }
                if (packetAwaiter.Task.IsFaulted || packetAwaiter.Task.IsCanceled)
                {
                    Console.WriteLine(packetAwaiter.Task.Exception);
                    cts.Cancel();
                }
            }

            for (int i = SendMessageAwaiters.Count - 1; i >= 0; i--)
            {
                var awaiter = SendMessageAwaiters[i];
                if (awaiter.Task.IsFaulted)
                {
                    Console.WriteLine(awaiter.Task.Exception);
                    SendMessageAwaiters.RemoveAt(i);
                }
                if (awaiter.Task.IsCompletedSuccessfully)
                {
                    SendMessageAwaiters.RemoveAt(i);
                }
            }
        }
    }
}
