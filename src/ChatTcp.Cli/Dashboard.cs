namespace CliChat.Cli;

internal struct Point
{
    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; set; }
    public int Y { get; set; }
}

internal class Dashboard
{
    private const int menuWidth = 25;
    private const int inputBoxHeight = 3;
    public Point curserInput {  get; private set; }

    public Dashboard(Action<CharElement> addCharElement, int height, int width)
    {
        AddDashboardElements(addCharElement, height, width);
        curserInput = new Point(1, height - inputBoxHeight + 1);
    }

    private static void AddDashboardElements(Action<CharElement> addCharElement, int height, int width)
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


        int verticalSplitX = width - menuWidth;
        AddVerticalLine(addCharElement, height - 1, verticalSplitX, 1, '│');

        int horizontalSplitY = height - inputBoxHeight;
        AddHorisontalLine(addCharElement, width - 1, 1, horizontalSplitY, '─');

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
