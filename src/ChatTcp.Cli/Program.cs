

using System.Linq.Expressions;

namespace CliChat.Cli;

internal class Program
{

    private static void Main(string[] args)
    {
        var consoleUi = new ConsoleUi();
        Dashboard.AddDashboardElements(consoleUi.AddCharElement, height: consoleUi.Height, width: consoleUi.Width);
        consoleUi.RenderScreen();
        Console.Read();
        //Console.WriteLine("Server (Y/n)");
        //var serverQ = Console.ReadLine()?.ToLower();
        //bool server = serverQ == "Y";

        //if (server)
        //{
        //    using var messagServer = new MessageServer();
        //    messagServer.Start();
        //    Environment.Exit(0);
        //}
        //else
        //{
        //    MessageClient.ConnectToServer();
        //}
    }
}
