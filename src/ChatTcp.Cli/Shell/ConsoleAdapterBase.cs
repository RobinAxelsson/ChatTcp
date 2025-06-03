
namespace ChatTcp.Cli.Shell;

internal abstract class ConsoleAdapterBase
{
    public abstract int WindowWidth { get; }
    public abstract int WindowHeight { get; }
    public abstract (int x, int y) GetCursorPosition();
    public abstract void ClearConsole();
    public abstract void SetCursorPosition(int x, int y);
    public abstract void Write(char c);
}
