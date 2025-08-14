using System.Net.Sockets;

namespace ChatTcp.Kernel.Resources;

public class KernelConnection : IDisposable
{
    public TcpClient TcpClient { get; }
    public NetworkStream NetworkStream { get; }

    public KernelConnection(TcpClient client)
    {
        TcpClient = client;
        NetworkStream = client.GetStream();
    }

    public void Dispose()
    {
        NetworkStream?.Dispose();
        TcpClient?.Dispose();
    }

    public override string ToString()
    {
        return $"|{TcpClient?.Client.LocalEndPoint}<->{TcpClient?.Client.RemoteEndPoint}|";
    }
}
