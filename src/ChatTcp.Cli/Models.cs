namespace ChatTcp.Cli;

public class TextLayer
{
    public TextLayerState State { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    public List<List<char>> LayerBuffer { get; } = new();

    public TextLayer(ConsoleColor foregroundColor, string text)
    {
        ForegroundColor = foregroundColor;
        MapTextToRows(text, LayerBuffer);
    }

    private static void MapTextToRows(string text, List<List<char>> layerBuffer)
    {
        layerBuffer.Clear();

        // Split into lines on \r\n or \n
        var lines = text.Replace("\r", "").Split('\n');

        foreach (var line in lines)
        {
            var row = new List<char>();

            foreach (var c in line)
            {
                row.Add(c);
            }

            layerBuffer.Add(row);
        }
    }
}

public enum TextLayerState
{
    Initialized,
    //Rendering - we use queue for this state
    Rendered
}

public record LayerChar
{
    public TextLayer TextLayer { get; }
    public char Char { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public LayerChar(TextLayer textLayer, char character, int x, int y)
    {
        TextLayer = textLayer;
        Char = character;
        X = x;
        Y = y;
    }
}
