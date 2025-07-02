namespace ChatTcp.Cli.Shell;

public struct Drawable
{
    public Drawable(int x, int y, char c)
    {
        X = x;
        Y = y;
        C = c;
    }

    public int X { get; set; }
    public int Y { get; set; }
    public char C { get; set; }
}
