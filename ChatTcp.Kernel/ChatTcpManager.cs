using System.Net;
using ChatTcp.Kernel.Resources;

namespace ChatTcp.Kernel;

public static class ChatTcpManager
{
    public static List<KernelListener> Listeners = new();
    public static List<TcpClientTask> AcceptTcpClientTasks = new();
    public static List<KernelConnection> Connections = new();
    public static List<SendMessageTask> SendMessageTasks = new();
    public static List<ReceiveMessageTask> ReceiveMessageTasks = new();

    public static void Run()
    {
        var cts = new CancellationTokenSource(); //world singleton

        var listeners = new List<KernelListener>
        {
            new(IPAddress.Loopback, 8888),
            new(IPAddress.Loopback, 8889)
        };

        while (!cts.IsCancellationRequested)
        {
            for (int i = listeners.Count() - 1; i >= 0; i--)
            {
                var listener = listeners[i];
                if (listener.State == ListenerState.Created)
                {
                    Console.WriteLine("{0} created", listener);
                    listener.Start();
                    Console.WriteLine("{0} started", listener);
                }
                else if (listener.State == ListenerState.Listening)
                {
                    int acceptOpsCount = AcceptTcpClientTasks.Where(x => x.Listener == listener).Count();
                    for (int j = 0; j < listener.ReceiversMax; j++)
                    {
                        if (acceptOpsCount + j < listener.ReceiversMax)
                        {
                            AcceptTcpClientTasks.Add(new TcpClientTask(listener, listener.Socket.AcceptTcpClientAsync(cts.Token)));
                            Console.WriteLine("{0} accept tcp client task created", listener);
                        }
                    }
                }
                else
                {
                    throw new ChatTcpKernelException("State not implimented: " + listener.State);
                }
            }

            for (int i = AcceptTcpClientTasks.Count - 1; i >= 0; i--)
            {
                var acceptTcpClientTask = AcceptTcpClientTasks[i];

                if (acceptTcpClientTask.Task.IsCompletedSuccessfully)
                {
                    var connection = new KernelConnection(acceptTcpClientTask.Task.Result);
                    Connections.Add(connection);
                    Console.WriteLine($"{connection} created");
                    AcceptTcpClientTasks.RemoveAt(i);

                    SendMessageTasks.Add(new SendMessageTask(connection, PacketStream.WritePacketAsync(new ChatMessageDto(acceptTcpClientTask.Listener.ToString(), $"Established connection"), connection.NetworkStream, cts.Token)));
                }

                if (acceptTcpClientTask.Task.IsFaulted)
                {
                    Console.WriteLine(acceptTcpClientTask.Listener + " faulted accept tcp client task");
                    Console.WriteLine(acceptTcpClientTask.Task.AsTask().Exception);
                    AcceptTcpClientTasks.RemoveAt(i);
                }

                if (acceptTcpClientTask.Task.IsCanceled)
                {
                    Console.WriteLine(acceptTcpClientTask.Listener + " canceled accept tcp client task");
                    AcceptTcpClientTasks.RemoveAt(i);
                }
            }

            for (int i = Connections.Count - 1; i >= 0; i--)
            {
                var conn = Connections[i];
                if(!ReceiveMessageTasks.Any(x => x.Transport == conn))
                {
                    ReceiveMessageTasks.Add(new ReceiveMessageTask(conn, PacketStream.ReadPacketAsync(conn.NetworkStream, cts.Token)));
                    Console.WriteLine(conn + " waiting on packet ");
                }
            }

            for (int i = ReceiveMessageTasks.Count - 1; i >= 0; i--)
            {
                var receiveMessageTask = ReceiveMessageTasks[i];

                if (receiveMessageTask.Task.IsCompletedSuccessfully)
                {
                    var packetDto = receiveMessageTask.Task.Result;
                    Console.WriteLine(receiveMessageTask.Transport + " received " + packetDto.GetType().Name + " " + packetDto.Id);

                    switch (packetDto)
                    {
                        case ChatMessageDto chat:
                            for (int j = Connections.Count - 1; j >= 0; j--)
                            {
                                var conn = Connections[j];
                                if (conn == receiveMessageTask.Transport)
                                    continue;

                                SendMessageTasks.Add(new SendMessageTask(conn, PacketStream.WritePacketAsync(chat, conn.NetworkStream, cts.Token)));
                                Console.WriteLine($"{conn} create send message task {chat}");
                            }
                            break;

                        case JoinChatDto joinChat:
                            Console.WriteLine($"{joinChat.Id} wants to join chat");
                            break;

                        default:
                            Console.WriteLine(receiveMessageTask.Transport + " invalid packet type received");
                            AcceptTcpClientTasks.RemoveAt(i);
                            break;
                    }

                    ReceiveMessageTasks.RemoveAt(i);
                }

                if (receiveMessageTask.Task.IsFaulted)
                {
                    Console.WriteLine(receiveMessageTask.Transport + "Receive message task faulted for connection");
                    Console.WriteLine(receiveMessageTask.Task.Exception);
                    ReceiveMessageTasks.RemoveAt(i);
                }

                if (receiveMessageTask.Task.IsCanceled)
                {
                    Console.WriteLine(receiveMessageTask.Transport + "Receive message task canceled for connection: ");
                    ReceiveMessageTasks.RemoveAt(i);
                }
            }
        }
    }
}
