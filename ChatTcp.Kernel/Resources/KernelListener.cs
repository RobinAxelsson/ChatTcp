using System.Net;
using System.Net.Sockets;

namespace ChatTcp.Kernel.Resources;

public class KernelListener : IDisposable
{
    public TcpListener Socket { get; }
    public IPAddress Address { get; }
    public int Port { get; }
    public int ReceiversMax { get; } = 3;
    public ListenerState State { get; private set; } = ListenerState.Created;
    public KernelListener(IPAddress address, int port)
    {
        Address = address;
        Port = port;
        Socket = new TcpListener(address, port);
    }

    public void Start()
    {
        if (State != ListenerState.Created && State != ListenerState.Stopped)
            throw new InvalidOperationException($"Cannot start listener from {State} state.");

        State = ListenerState.Starting;
        try
        {
            Socket.Start();
            State = ListenerState.Listening;
        }
        catch
        {
            State = ListenerState.Faulted;
            throw;
        }
    }

    public void Stop()
    {
        if (State == ListenerState.Listening)
        {
            State = ListenerState.Stopping;
            Socket.Stop();
            State = ListenerState.Stopped;
        }
    }

    public void Dispose()
    {
        Socket.Stop();
        Socket.Dispose();
        State = ListenerState.Disposed;
    }

    public override string ToString()
    {
        return $"|{Address}:{Port}:{State}|";
    }
}

public enum ListenerState
{
    Created,       // Allocated but not started
    Starting,      // Start() called but not yet accepting (optional transitional)
    Listening,     // Accepting connections
    Stopping,      // Stop requested but accepts still completing
    Stopped,       // Fully stopped
    Faulted,       // Hit an unrecoverable error
    Disposed       // Freed resources
}
