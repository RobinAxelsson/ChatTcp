using System.Net.Sockets;

namespace ChatTcp.Cli;

public readonly record struct TcpClientTask
{
    public Listener Listener { get; }
    public ValueTask<TcpClient> Task { get; }

    public TcpClientTask(Listener listener, ValueTask<TcpClient> task)
    {
        Listener = listener;
        Task = task;
    }
}

public readonly record struct ReceiveMessageTask
{
    public Connection Transport { get; }
    public Task<WirePacketDto> Task { get; }

    public ReceiveMessageTask(Connection transport, Task<WirePacketDto> task)
    {
        Transport = transport;
        Task = task;
    }
}

public readonly record struct SendMessageTask
{
    public Connection Transport { get; }
    public Task Task { get; }

    public SendMessageTask(Connection transport, Task task)
    {
        Transport = transport;
        Task = task;
    }
}

