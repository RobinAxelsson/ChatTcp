namespace CliChat.Cli;

internal class Dashboard
{
    public static void AddDashboardElements(Action<CharElement> addCharElement, int height, int width, int xStart = 0, int yStart = 0)
    {
        /*
         * ┌──────────────1──────────────┬────────┐
         * │Hello!                       │  menu  │
         * │                      Hi man!│        │
         * │                             │        │
         * 2                             3        4
         * │                             │        │
         * │                             │        │
         * │                             │        │
         * ├──────────────5──────────────┤        │
         * │        1 row input          │        │
         * └──────────────6──────────────┴────────┘
         */

        int xEnd = xStart + width - 1;
        int yEnd = yStart + height - 1;
        const int menuWidth = 25;
        const int inputHeight = 3;

        int verticalSplitX = xEnd - menuWidth;
        int horizontalSplitY = yEnd - inputHeight;

        int line1Length = width - 1;
        int line1xStart = xStart + 1;
        AddHorisontalLine(addCharElement, line1Length, line1xStart, 0, '─');

        int line2Length = height - 1;
        AddVerticalLine(addCharElement, line2Length, xStart, yStart + 1, '│');

        int line3Length = line2Length;
        AddVerticalLine(addCharElement, line3Length, verticalSplitX, yStart + 1, '│');

        int line4length = line2Length;
        AddVerticalLine(addCharElement, line4length, xEnd, yStart + 1, '│');

        int line5Length = width - menuWidth - 1;
        AddHorisontalLine(addCharElement, line5Length, xStart + 1, horizontalSplitY, '─');

        int line6Length = line1Length;
        int line6xStart = line1xStart;
        AddHorisontalLine(addCharElement, line6Length, line6xStart, yEnd, '─');

        AddChar(addCharElement, xStart, yStart, '┌');
        AddChar(addCharElement, verticalSplitX, yStart, '┬');
        AddChar(addCharElement, xEnd, yStart, '┐');
        AddChar(addCharElement, xStart, horizontalSplitY, '├');
        AddChar(addCharElement, verticalSplitX, horizontalSplitY, '┤');
        AddChar(addCharElement, xStart, yEnd, '└');
        AddChar(addCharElement, verticalSplitX, yEnd, '┴');
        AddChar(addCharElement, xEnd, yEnd, '┘');


    }

    private static void AddChar(Action<CharElement> addCharElement, int x, int y, char c)
    {
        addCharElement(new CharElement() { Char = c, X = x, Y = y });
    }

    private static void AddVerticalLine(Action<CharElement> addCharElement, int length, int x, int yStart, char c)
    {
        for (int y = 0; y < length; y++)
        {
            addCharElement(new CharElement() { Char = c, X = x, Y = y + yStart });
        }
    }

    private static void AddHorisontalLine(Action<CharElement> addCharElement, int length, int xStart, int y, char c)
    {
        for (int x = 0; x < length; x++)
        {
            addCharElement(new CharElement() { Char = c, X = xStart + x, Y = y });
        }
    }
}
