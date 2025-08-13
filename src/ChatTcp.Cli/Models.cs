namespace ChatTcp.Cli;

public class TextLayer
{
    public TextLayerState State { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    internal string Text { get; set; }
    public List<List<char>> Rows { get; } = new();

    public TextLayer(ConsoleColor foregroundColor, string text)
    {
        ForegroundColor = foregroundColor;
        Text = text ?? string.Empty;
        MapTextToRows();
    }

    private void MapTextToRows()
    {
        Rows.Clear();

        // Split into lines on \r\n or \n
        var lines = Text.Replace("\r", "").Split('\n');

        foreach (var line in lines)
        {
            var row = new List<char>();

            foreach (var c in line)
            {
                row.Add(c);
            }

            Rows.Add(row);
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
