namespace ChatTcp.Cli;

internal class ConsoleAdapter : IConsoleAdapter
{
    private static ConsoleAdapter? _instance = null;
    private Action<int, int> _setCursorPosition = SetCursorPositionCore;
    private Action _clear = Console.Clear;
    private ConsoleAdapter() { }

    internal static ConsoleAdapter Instance => _instance ??= new ConsoleAdapter();

    //test constructor
    internal ConsoleAdapter(Action<int, int> setCursorPosition, Action clear)
    {
        _setCursorPosition = setCursorPosition;
        _clear = clear;
    }

    //public void SetCursorPosition(int x, int y) => _setCursorPosition?.Invoke(x, y);

    public void SetCursorPosition(int x, int y)
    {
        _setCursorPosition(x, y);
    }

    private static void SetCursorPositionCore(int x, int y)
    {
        if (x < 0 || y < 0) throw new ArgumentOutOfRangeException();

        // Grow height first (must be >= WindowHeight)
        if (y >= Console.BufferHeight)
        {
            int newHeight = Math.Max(y + 9000, Console.WindowHeight);
            Console.BufferHeight = newHeight;
        }
        // Grow width if needed
        if (x >= Console.BufferWidth)
        {
            int newWidth = Math.Max(x + 1, Console.WindowWidth);
            Console.BufferWidth = newWidth;
        }

        Console.SetCursorPosition(x, y);
    }

    public void Clear() => _clear?.Invoke();
}
