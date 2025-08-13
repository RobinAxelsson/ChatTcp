namespace ChatTcp.Cli;

public class TextLayer
{
    public TextLayerState State { get; set; }
    public ConsoleColor ForegroundColor { get; set; }
    public string Text { get; set; }

    public TextLayer(ConsoleColor foregroundColor, string text)
    {
        ForegroundColor = foregroundColor;
        Text = text ?? string.Empty;
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
