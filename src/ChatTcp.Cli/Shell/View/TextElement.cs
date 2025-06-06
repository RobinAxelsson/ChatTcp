namespace ChatTcp.Cli.Shell.View;

internal class TextElement
{
    public int X { get; set; }
    public int Y { get; set; }
    private readonly string _text;

    private int _maxWidth;
    public int Width => CalculateWidth();
    public int Height => CalculateHeight();
    public TextElement(string text, int maxWidth)
    {
        _text = text;
        _maxWidth = maxWidth;
    }

    private int CalculateWidth()
    {
        string Clean(string input) =>
            new string(input.Where(c => !char.IsControl(c) || c == '\n').ToArray());

        var cleanedText = Clean(_text);

        int maxLineLength = cleanedText
            .Split('\n')
            .Select(line => line.Length)
            .Max();

        return maxLineLength > _maxWidth ? _maxWidth : maxLineLength;
    }


    private int CalculateHeight()
    {
        int height = 1;
        int lineLength = 0;

        for (int i = 0; i < _text.Length; i++)
        {
            char c = _text[i];

            if (c == '\t' || c == '\r')
                continue;

            if (c == '\n')
            {
                height++;
                lineLength = 0;
                continue;
            }

            lineLength++;

            if (lineLength == _maxWidth)
            {
                height++;
                lineLength = 0;
            }
        }

        return height;
    }

    public List<Drawable> GetDrawables()
    {
        var drawables = new List<Drawable>();

        int textLength = _text.Length;

        int y = Y;
        int x = X;
        int lineIndex = 0;
        int chrPtr = 0;
        while (chrPtr < textLength)
        {
            //If line wrap is needed
            if (lineIndex == _maxWidth)
            {
                y++;
                x = X;
                lineIndex = 0;
            }

            char c = _text[chrPtr];

            if (c == '\t' || c == '\r')
            {
                //just skip
                chrPtr++;
                continue;
            }

            if (c == '\n')
            {
                x = X;
                y++;
                chrPtr++;
                lineIndex = 0;
            }

            drawables.Add(new Drawable(x, y, c));

            x++;
            chrPtr++;
            lineIndex++;
        }

        return drawables;
    }
}
