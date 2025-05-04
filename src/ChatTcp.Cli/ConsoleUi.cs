namespace CliChat.Cli;

internal class ConsoleUi
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
        var curser = ConsoleHelper.GetCurserPosition();

        for (int x = 0; x < bufferX; x++)
        {
            for (int y = 0; y < bufferY; y++)
            {
                if(_nextScreen[x, y] != _renderedScreen[x, y])
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(_nextScreen[x, y]);
                    _renderedScreen[x, y] = _nextScreen[x, y];
                }
            }
        }

        ConsoleHelper.SetCurserPosition(curser);
    }
}

internal static class ConsoleHelper
{
    public static Point GetCurserPosition()
    {
        var curserPosition = Console.GetCursorPosition();
        return new Point(curserPosition.Left, curserPosition.Top);
    }

    public static void SetCurserPosition(Point point)
    {
        Console.SetCursorPosition(point.X, point.Y);
    }
}

internal struct CharElement
{
    public char Char { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}
