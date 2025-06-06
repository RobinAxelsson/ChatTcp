namespace ChatTcp.Cli.Shell.View;

internal sealed class Display
{
    private char [,] _drawn;
    private char [,] _next;
    private readonly int _width;
    private readonly int _height;

    public Display(int width, int height)
    {
        _width = width;
        _height = height;
        _drawn = new char[_width, _height];
        _next = new char[_width, _height];
    }

    public void ClearArea(int x0, int y0, int x1, int y1)
    {
        if (x0 < 1 || y0 < 1 || x0 > x1 || y0 > y1 || x1 >= _width || y1 >= _height)
        {
            string message = $"Invalid coordinates: x0={x0}, y0={y0}, x1={x1}, y1={y1}, " +
                             $"bounds=({_width}, {_height})";

            throw new ShellException(message);
        }


        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                _next[x, y] = '\0';
            }
        }
    }


    public void Clear(IEnumerable<Drawable> drawables)
    {
        ValidateOverflow(drawables);

        foreach (var d in drawables)
        {
            _next[d.X, d.Y] = '\0';
        }
    }

    public void Add(IEnumerable<Drawable> drawables)
    {
        ValidateOverflow(drawables);

        foreach (var d in drawables)
        {
            _next[d.X, d.Y] = d.C;
        }
    }

    private void ValidateOverflow(IEnumerable<Drawable> drawables)
    {
        bool isOverflowX = drawables.Any(d => d.X >= _width);
        bool isOverflowY = drawables.Any(d => d.Y >= _height);

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
                $"Display too small. Width: {_width}, Height: {_height}, " +
                $"drawable MaxX: {maxX}, drawable MaxY: {maxY}, " +
                $"Overflow drawables: '{overflowChars}'");
        }
    }

    public void Render()
    {
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
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
                }
            }
        }
    }
}
