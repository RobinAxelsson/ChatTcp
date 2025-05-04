namespace CliChat.Cli;

internal class Dashboard
{
    public static void AddDashboardElements(Action<CharElement> addCharElement, int height, int width, int xStart = 0, int yStart = 0)
    {
        /*
         *|<-             width                ->|
         * Bob: Hello!                  │  menu
         *                       Hi man!│<-    ->|   
         *                              │      
         *                              │      
         *                              │      
         *                              │      
         *                              │      
         * ─────────────────────────────┼────────     
         *         input box            │      
         *                              │
         */

        const int menuWidth = 25;
        const int inputBoxHeight = 3;

        int verticalSplitX = width - menuWidth;
        AddVerticalLine(addCharElement, height - 1, verticalSplitX, yStart + 1, '│');

        int horizontalSplitY = height - inputBoxHeight;
        AddHorisontalLine(addCharElement, width - 1, xStart + 1, horizontalSplitY, '─');

        AddChar(addCharElement, verticalSplitX, horizontalSplitY, '┼');
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
