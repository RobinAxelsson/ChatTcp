
using System.Text;

namespace ChatTcp.Cli;
internal static class Program
{

    public static Dictionary<ConsoleKey, TextLayer> KeyTextLayerDict = new();
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Started");

        var cts = new CancellationTokenSource();

        var textLayers = new List<TextLayer>
        {
            new TextLayer(ConsoleColor.Magenta, Text.LoremIpsum20Lines),
            new TextLayer(ConsoleColor.DarkRed, Text.AsciiTable),
            new TextLayer(ConsoleColor.Cyan, Text.CodeComment),
            new TextLayer(ConsoleColor.Yellow, Text.NoteAcceptOp),
            new TextLayer(ConsoleColor.White, Text.OneToHundredWords)
        };

        var renderSystem = new RenderSystem(textLayers);

        KeyTextLayerDict[ConsoleKey.D1] = textLayers[0];
        KeyTextLayerDict[ConsoleKey.D2] = textLayers[1];
        KeyTextLayerDict[ConsoleKey.D3] = textLayers[2];
        KeyTextLayerDict[ConsoleKey.D4] = textLayers[3];
        KeyTextLayerDict[ConsoleKey.D5] = textLayers[4];

        var sb = new StringBuilder();

        while (!cts.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var keyInfo = Console.ReadKey(true);
                if (keyInfo.Key == ConsoleKey.Escape)
                {
                    cts.Cancel();
                }
                if (keyInfo.Key == ConsoleKey.Enter)
                {
                    renderSystem.RequestClear();
                }
                if (KeyTextLayerDict.TryGetValue(keyInfo.Key, out TextLayer? textLayerValue))
                {
                    renderSystem.RequestRender(textLayerValue);
                }
            }

            renderSystem.Tick();
        }

        Console.WriteLine("Exiting...");
    }
}
