namespace ChatTcp.Cli.ConsoleUi;

internal class Frame
{
    //private Action<CharElement> _addCharElement;
    //private AppWindow _appWindow;
    public Frame(Action<CharElement> addCharElement, AppWindow appWindow)
    {
        //_addCharElement = addCharElement;
        //_appWindow = appWindow;
        AddElements(addCharElement, appWindow.Height, appWindow.Width);
    }

    private static void AddElements(Action<CharElement> addCharElement, int height, int width)
    {
        /*
         *|<-             Width                ->|
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


        int verticalSplitX = width - AppWindow.MENU_WIDTH;
        AddVerticalLine(addCharElement, height - 1, verticalSplitX, 1, '│');

        int horizontalSplitY = height - AppWindow.INPUT_HEIGHT;
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
