namespace CliChat.Cli;

internal class ConsoleUi
{
    private List<List<char>> _renderedScreen = new();
    private List<List<char>> _nextScreen = new();
    public int Width { get; private set; } = 0;
    public int Height { get; private set; } = 0;

    public ConsoleUi()
    {
        SetScreenSize(25, 100);
    }

    internal void AddCharElement(CharElement charElement)
    {
        if(charElement.X > Width || charElement.Y > Height)
        {
            throw new ArgumentException($"{nameof(CharElement)} out of range, y = {charElement.X}, x = {charElement.Y}");
        }

        _nextScreen[charElement.X][charElement.Y] = charElement.Char;
    }

    internal void SetScreenSize(int newHeight, int newWidth)
    {
        for (int x = Width; x < newWidth; x++)
        {
            var renderedColumn = new List<char>();
            var newColumn = new List<char>();

            for (int y = Height; y < newHeight; y++)
            {
                renderedColumn.Add('\0');
                newColumn.Add('\0');
            }

            _renderedScreen.Add(renderedColumn);
            _nextScreen.Add(newColumn);
        }

        Height = newHeight;
        Width = newWidth;

        if(_renderedScreen.Count != Width)
        {
            throw new ArgumentException($"columns: {_renderedScreen.Count} does not match length: {Width}");
        }

        if(_renderedScreen[0].Count != Height)
        {
            throw new ArgumentException($"rows: {_renderedScreen[0].Count} does not match length: {Height}");
        }
    }

    internal void RenderScreen()
    {
        for (int x = 0; x < _nextScreen.Count; x++)
        {
            for (int y = 0; y < _nextScreen[x].Count; y++)
            {
                if(_nextScreen[x][y] != _renderedScreen[x][y])
                {
                    Console.SetCursorPosition(x, y);
                    Console.Write(_nextScreen[x][y]);
                    _renderedScreen[x][y] = _nextScreen[x][y];
                }
            }
        }
    }
}

internal struct CharElement
{
    public char Char { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}
