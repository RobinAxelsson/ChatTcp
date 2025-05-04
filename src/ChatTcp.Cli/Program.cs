
namespace CliChat.Cli;

internal class Program
{

    private static void Main(string[] args)
    {
        Console.WriteLine("Server (y/n)");
        var serverQ = Console.ReadLine()?.ToLower();
        bool server = serverQ == "y";

        if (server)
        {
            using var messagServer = new MessageServer();
            messagServer.Start();
            Environment.Exit(0);
        }
        else
        {
            MessageClient.ConnectToServer();
        }
    }
}
