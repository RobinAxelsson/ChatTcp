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
        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                _next[x,y] = '\0';
            }
        }
    }

    public void Clear(IEnumerable<Drawable> drawables)
    {
        foreach (var d in drawables)
        {
            _next[d.X, d.Y] = '\0';
        }
    }

    public void Add(IEnumerable<Drawable> drawables)
    {
        foreach (var d in drawables)
        {
            _next[d.X, d.Y] = d.C;
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
                }
            }
        }
    }
}
