namespace ChatTcp.Cli.Shell;

public struct Glyph
{
    public Glyph(int x, int y, char c)
    {
        X = x;
        Y = y;
        C = c;
    }

    public int X { get; set; }
    public int Y { get; set; }
    public char C { get; set; }
}
