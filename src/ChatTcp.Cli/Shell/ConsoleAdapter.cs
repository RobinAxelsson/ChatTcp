namespace ChatTcp.Cli.Shell;

internal sealed class ConsoleAdapter : ConsoleAdapterBase
{
    public override int WindowWidth => Console.WindowWidth;
    public override int WindowHeight => Console.WindowHeight;

    public override void ClearConsole() => Console.Clear();

    public override (int x, int y) GetCursorPosition()
    {
        return Console.GetCursorPosition();
    }

    public override void SetCursorPosition(int x, int y)
    {
        Console.SetCursorPosition(x, y);
    }

    public override void Write(char c)
    {
        Console.Write(c);
    }
}
