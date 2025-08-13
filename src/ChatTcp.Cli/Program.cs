
namespace ChatTcp.Cli;
internal static class Program
{

    public static Dictionary<ConsoleKey, TextLayer> KeyTextLayerDict = new();
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Started");

        var cts = new CancellationTokenSource();

        var networkScreen = new ScreenViewModel(ConsoleColor.Magenta, "Network log");
        var chatView = new ScreenViewModel(ConsoleColor.DarkRed, "Chat");
        var inputField = new ScreenViewModel(ConsoleColor.Yellow, "InputField");

        var networkSystem = new NetworkSystem(networkScreen);
        //networkManager.Start()


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

    private static void MainControlFlow(CancellationTokenSource cts)
    {
        var keyInfo = Console.ReadKey(true);
        if (keyInfo.Key == ConsoleKey.Escape)
        {
            cts.Cancel();
        }
        if (keyInfo.Key == ConsoleKey.D1 && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            //set network state
            //request render if new state
            return;
        }
        if (keyInfo.Key == ConsoleKey.D1 && keyInfo.Modifiers == ConsoleModifiers.Control)
        {
            //set chatView state
            //request render if new state
            return;
        }

        //send key to application flows
    }
}
