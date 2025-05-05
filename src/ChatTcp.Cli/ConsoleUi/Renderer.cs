namespace ChatTcp.Cli.ConsoleUi;

internal class Renderer
{
    private const int bufferX = 500;
    private const int bufferY = 200;
    private char[,] _renderedScreen = new char[bufferX, bufferY];
    private char[,] _nextScreen = new char[bufferX, bufferY];

    internal void AddCharElement(CharElement charElement)
    {
        _nextScreen[charElement.X, charElement.Y] = charElement.Char;
    }

    internal void RenderScreen()
    {
        var curser = GetCursorPosition();

        for (int x = 0; x < bufferX; x++)
        {
            for (int y = 0; y < bufferY; y++)
            {
                if (_nextScreen[x, y] != _renderedScreen[x, y])
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(_nextScreen[x, y]);
                    _renderedScreen[x, y] = _nextScreen[x, y];
                }
            }
        }

        SetCursorPosition(curser);
    }

    private static Point GetCursorPosition()
    {
        var curserPosition = Console.GetCursorPosition();
        return new Point(curserPosition.Left, curserPosition.Top);
    }

    private static void SetCursorPosition(Point point)
    {
        Console.SetCursorPosition(point.X, point.Y);
    }
}

internal struct CharElement
{
    public required char Char { get; init; }
    public required int X { get; init; }
    public required int Y { get; init; }
}
