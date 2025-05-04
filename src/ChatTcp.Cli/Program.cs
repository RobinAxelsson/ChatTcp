

using System.Linq.Expressions;

namespace CliChat.Cli;

internal class Program
{

    private static void Main(string[] args)
    {
        var consoleUi = new ConsoleUi();
        var width = Console.WindowWidth;
        var height = Console.WindowHeight;
        var dashboard = new Dashboard(consoleUi.AddCharElement, height: height, width: width);
        Console.SetCursorPosition(dashboard.curserInput.X, dashboard.curserInput.Y);
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
