
namespace ChatTcp.Cli.Shell;

internal abstract class ConsoleAdapterBase
{
    public abstract int WindowWidth { get; }
    public abstract int WindowHeight { get; }
    public abstract void Draw(int x, int y, char c);
    public abstract (int x, int y) GetCursorPosition();
    public abstract void ClearConsole();
    public abstract void SetCursorPosition(int x, int y);
}
