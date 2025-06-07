using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell.View;

internal class PromptArea
{
    public PromptArea(int x0, int y0, int x1, int y1)
    {
        if (x0 >= x1)
            throw new ShellException($"x0: {x0} can not be bigger then or equal to x1: {x1}");

        if (y0 >= y1)
            throw new ShellException($"y0: {y0} can not be bigger then or equal to y1: {y1}");

        X0 = x0;
        Y0 = y0;
        X1 = x1;
        Y1 = y1;
    }

    public int X0 { get; set; }
    public int Y0 { get; set; }
    public int X1 { get; set; }
    public int Y1 { get; set; }

    public Drawable[] GetDrawables(string prompt, int cursorIndex)
    {
        var text = ShellSettings.Prompt + prompt + ' '; //Extra for cursor

        int y = Y0;
        int width = X1 - X0;

        var textElement = new TextElement(text, width);

        textElement.Y = y;
        textElement.X = X0;

        y += textElement.Height;

        var drawables = textElement.GetDrawables().ToArray();

        int maxY = drawables.Max(x => x.Y);
        int diff = Y1 - maxY;

        if (diff < 0)
        {
            for (int i = 0; i < drawables.Length; i++)
            {
                drawables[i].Y += diff; //+-
            }
        }

        drawables = [.. drawables.Where(x => x.Y >= Y0)];

        return drawables;
    }
}
