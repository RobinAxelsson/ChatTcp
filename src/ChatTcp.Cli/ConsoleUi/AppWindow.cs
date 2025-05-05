namespace ChatTcp.Cli.ConsoleUi;

internal class AppWindow
{
    internal const int MENU_WIDTH = 25;
    internal const int INPUT_HEIGHT = 3;
    public int Width { get; private set; }
    public int Height { get; private set; }

    public AppWindow()
    {
        Width = Console.WindowWidth;
        Height = Console.WindowHeight;
    }

    public (Point start, Point end) GetInputArea()
    {
        var start = new Point(1, Height - INPUT_HEIGHT + 1);
        var end = new Point(Width - MENU_WIDTH - 1, Height);

        return (start, end);
    }

    public (Point start, Point end) GetTextLoc()
    {
        var start = new Point(1, 1);
        var end = new Point(Width - MENU_WIDTH - 1, Height - INPUT_HEIGHT - 1);

        return (start, end);
    }
}
