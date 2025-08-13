using System.Net;
using ChatTcp.Cli.Shell;
using ChatTcp.Kernel;
using ChatTcp.Kernel.Resources;

namespace ChatTcp.Cli;

internal class NetworkSystem
{
    private readonly ScreenViewModel _networkScreen;
    public List<Listener> Listeners = new();
    public List<TcpClientTask> AcceptTcpClientTasks = new();
    public List<Connection> Connections = new();
    public List<SendMessageTask> SendMessageTasks = new();
    public List<ReceiveMessageTask> ReceiveMessageTasks = new();

    public NetworkSystem(ScreenViewModel networkScreen)
    {
        _networkScreen = networkScreen;
    }

    public void Start(CancellationToken token)
    {
        var listeners = new List<Listener>
        {
            new(IPAddress.Loopback, 8888),
            new(IPAddress.Loopback, 8889)
        };

        for (int i = listeners.Count() - 1; i >= 0; i--)
        {
            var listener = listeners[i];
            if (listener.State == ListenerState.Created)
            {
                _networkScreen.AppendLine($"{listener} created");
                listener.Start();
                _networkScreen.AppendLine($"{listener} started");
            }
            else if (listener.State == ListenerState.Listening)
            {
                int acceptOpsCount = AcceptTcpClientTasks.Where(x => x.Listener == listener).Count();
                for (int j = 0; j < listener.ReceiversMax; j++)
                {
                    if (acceptOpsCount + j < listener.ReceiversMax)
                    {
                        AcceptTcpClientTasks.Add(new TcpClientTask(listener, listener.Socket.AcceptTcpClientAsync(token)));
                        _networkScreen.AppendLine(listener + " accept tcp client task created");
                    }
                }
            }
            else
            {
                throw new InvalidStateException("State not implimented: " + listener.State);
            }
        }

        for (int i = AcceptTcpClientTasks.Count - 1; i >= 0; i--)
        {
            var acceptTcpClientTask = AcceptTcpClientTasks[i];

            if (acceptTcpClientTask.Task.IsCompletedSuccessfully)
            {
                var connection = new Connection(acceptTcpClientTask.Task.Result);
                Connections.Add(connection);
                _networkScreen.AppendLine($"{connection} created");
                AcceptTcpClientTasks.RemoveAt(i);

                SendMessageTasks.Add(new SendMessageTask(connection, PacketStream.WritePacketAsync(new ChatMessageDto(acceptTcpClientTask.Listener.ToString(), $"Established connection"), connection.NetworkStream, token)));
            }

            if (acceptTcpClientTask.Task.IsFaulted)
            {
                _networkScreen.AppendLine(acceptTcpClientTask.Listener + " faulted accept tcp client task");
                _networkScreen.AppendLine(acceptTcpClientTask.Task.AsTask().Exception?.ToString());
                AcceptTcpClientTasks.RemoveAt(i);
            }

            if (acceptTcpClientTask.Task.IsCanceled)
            {
                _networkScreen.AppendLine(acceptTcpClientTask.Listener + " canceled accept tcp client task");
                AcceptTcpClientTasks.RemoveAt(i);
            }
        }

        for (int i = Connections.Count - 1; i >= 0; i--)
        {
            var conn = Connections[i];
            if (!ReceiveMessageTasks.Any(x => x.Transport == conn))
            {
                ReceiveMessageTasks.Add(new ReceiveMessageTask(conn, PacketStream.ReadPacketAsync(conn.NetworkStream, token)));
                _networkScreen.AppendLine(conn + " waiting on packet ");
            }
        }

        for (int i = ReceiveMessageTasks.Count - 1; i >= 0; i--)
        {
            var receiveMessageTask = ReceiveMessageTasks[i];

            if (receiveMessageTask.Task.IsCompletedSuccessfully)
            {
                var packetDto = receiveMessageTask.Task.Result;
                _networkScreen.AppendLine(receiveMessageTask.Transport + " received " + packetDto.GetType().Name + " " + packetDto.Id);

                switch (packetDto)
                {
                    case ChatMessageDto chat:
                        for (int j = Connections.Count - 1; j >= 0; j--)
                        {
                            var conn = Connections[j];
                            if (conn == receiveMessageTask.Transport)
                                continue;

                            SendMessageTasks.Add(new SendMessageTask(conn, PacketStream.WritePacketAsync(chat, conn.NetworkStream, token)));
                            _networkScreen.AppendLine($"{conn} create send message task {chat}");
                        }
                        break;

                    case JoinChatDto joinChat:
                        _networkScreen.AppendLine($"{joinChat.Id} wants to join chat");
                        break;

                    default:
                        _networkScreen.AppendLine(receiveMessageTask.Transport + " invalid packet type received");
                        AcceptTcpClientTasks.RemoveAt(i);
                        break;
                }

                ReceiveMessageTasks.RemoveAt(i);
            }

            if (receiveMessageTask.Task.IsFaulted)
            {
                _networkScreen.AppendLine(receiveMessageTask.Transport + "Receive message task faulted for connection");
                _networkScreen.AppendLine(receiveMessageTask.Task.Exception.ToString());
                ReceiveMessageTasks.RemoveAt(i);
            }

            if (receiveMessageTask.Task.IsCanceled)
            {
                _networkScreen.AppendLine(receiveMessageTask.Transport + "Receive message task canceled for connection: ");
                ReceiveMessageTasks.RemoveAt(i);
            }
        }
    }
}
