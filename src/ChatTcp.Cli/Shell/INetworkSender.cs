namespace ChatTcp.Cli.Shell;

internal interface INetworkSender
{
    public Task SendAsync(string message);
}
