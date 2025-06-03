namespace ChatTcp.Cli.Shell;

internal sealed class ConsoleAdapter : ConsoleAdapterBase
{
    public override int WindowWidth => Console.WindowWidth;
    public override int WindowHeight => Console.WindowHeight;

    public override void ClearConsole() => Console.Clear();

    public override void Draw(int x, int y, char c)
    {
        Console.WriteLine(c);
    }

    public override (int x, int y) GetCursorPosition()
    {
        return Console.GetCursorPosition();
    }
}
