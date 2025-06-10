using static System.Net.Mime.MediaTypeNames;

namespace ChatTcp.Cli.Shell.View;

internal sealed class Display
{
    private char [,] _drawn;
    private char [,] _next;
    private int _width;
    private int _height;

    public int Height
    {
        get => _height;
        set
        {
            _height = value;
        }
    }

    public int Width
    {
        get
        {
            return _width;
        }
        set
        {
            _width = value;
        }
    }

    public Display(int width, int height)
    {
        _width = width;
        _height = height;
        _drawn = new char[_width, _height];
        _next = new char[_width, _height];
    }

    public void Add(IEnumerable<Drawable> drawables)
    {
        ValidateOverflow(drawables);

        foreach (var d in drawables)
        {
            _next[d.X, d.Y] = d.C;
        }
    }

    public void ClearArea(int x0, int y0, int x1, int y1)
    {
        for (int y = y0; y < y1; y++)
        {
            for (int x = x0; x < x1; x++)
            {
                Console.SetCursorPosition(x, y);
                Console.Write(' ');
                _drawn[x, y] = '\0';
                _next[x, y] = '\0';
            }
        }
    }

    private void ValidateOverflow(IEnumerable<Drawable> drawables)
    {
        bool isOverflowX = drawables.Any(d => d.X >= Width);
        bool isOverflowY = drawables.Any(d => d.Y >= Height);

        if (isOverflowX || isOverflowY)
        {
            int maxX = drawables.Max(d => d.X);
            int maxY = drawables.Max(d => d.Y);

            string overflowChars = new string(drawables
                .OrderBy(d => d.Y)
                .ThenBy(d => d.X)
                .Select(d => d.C)
                .ToArray());

            throw new ShellException(
                $"Display too small. Width: {Width}, Height: {Height}, " +
                $"drawable MaxX: {maxX}, drawable MaxY: {maxY}, " +
                $"Overflow drawables: '{overflowChars}'");
        }
    }

    public void Render()
    {
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var drawn = _drawn[x, y];
                var next = _next[x, y];
                if(drawn != next)
                {
                    Console.SetCursorPosition(x, y);

                    if(next == '\0')
                    {
                        Console.Write(' ');
                    }
                    else
                    {
                        Console.Write(next);
                    }

                    _drawn[x, y] = next;
                    _next[x, y] = '\0';
                }
            }
        }
    }
}
