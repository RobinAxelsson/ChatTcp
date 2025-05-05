using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal class Program
{

    private static void Main(string[] args)
    {
        var consoleUi = new Renderer();
        var appWindow = new AppWindow();
        var dashboard = new Frame(consoleUi.AddCharElement, appWindow);
        var chat = new Chat(consoleUi.AddCharElement, appWindow);
        consoleUi.RenderScreen();

        while (true)
        {
            InputBox.Activate(appWindow.GetInputArea(), chat.SubmitHostMessage);
            consoleUi.RenderScreen();
        }
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

