
using System.Net;
using ChatTcp.Kernel.Resources;

namespace ChatTcp.Cli;
internal static class Program
{

    public static async Task Main(string[] args)
    {
        Console.WriteLine("Started");

        var cts = new CancellationTokenSource();

        var networkSystem = new NetworkSystem(new List<Listener>
        {
            new(IPAddress.Loopback, 8888),
            new(IPAddress.Loopback, 8889)
        });

        var entityStore = new EntityStore();


        while (!cts.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                MainControlFlow(cts);
            }

            //renderSystem.Tick(); Blocking?
        }

        Console.WriteLine("Exiting...");
    }

    private static int selectedLine = 0;
    private static void MainControlFlow(CancellationTokenSource cts)
    {
        var keyInfo = Console.ReadKey(true);
        if (keyInfo.Key == ConsoleKey.Escape)
        {
            cts.Cancel();
        }
        if (keyInfo.Key == ConsoleKey.D1 && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            ConsoleWriter.Instance.WriteText(Text.LoremIpsum20Lines, 0);
            //set network state
            //request render if new state
            return;
        }
        if (keyInfo.Key == ConsoleKey.D2 && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            //var lines = Text.AsciiTable.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            //int lineNumber = 0;

            for (int i = 0; i < 12000; i++)
            {
                ConsoleWriter.Instance.WriteText(i.ToString(), i);
            }

            //set chatView state
            //request render if new state
            return;
        }
        if (keyInfo.Key == ConsoleKey.UpArrow)
        {
            selectedLine++;
            ConsoleWriter.Instance.WriteText(selectedLine.ToString(), 0);
        }
        if (keyInfo.Key == ConsoleKey.DownArrow)
        {
            selectedLine--;
            ConsoleWriter.Instance.WriteText(selectedLine.ToString(), 0);
        }
        if (keyInfo.Key == ConsoleKey.L)
        {
            ConsoleWriter.Instance.ClearLine(selectedLine);
        }
        if (keyInfo.Key == ConsoleKey.H)
        {
            ConsoleWriter.Instance.WriteText("hello world", selectedLine);
        }
        if (keyInfo.Key == ConsoleKey.Enter)
        {
            Console.Clear();
        }

        //send key to application flows
    }
}
