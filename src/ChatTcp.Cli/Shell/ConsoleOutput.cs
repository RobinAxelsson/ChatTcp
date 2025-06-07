using ChatTcp.Cli.Shell.View;

namespace ChatTcp.Cli.Shell;

internal class ConsoleOutput
{
    private ChatArea? _chatWindow;
    private Display _display;
    private PromptArea? _promptArea;

    public ConsoleOutput()
    {
        _display = new Display(300, 200); //this is just to make sure we do not get out of bounds
    }

    internal ConsoleOutput(Display display)
    {
        _display = display;
    }

    public void Handle(AppState appState)
    {

        _chatWindow = new ChatArea(0, 0, appState.WindowWidth, appState.WindowHeight-10);
        _promptArea = new PromptArea(0, appState.WindowHeight - 9, appState.WindowWidth, appState.WindowHeight);

        var drawables = _chatWindow.GetDrawables(appState.Messages);
        _display.Add(drawables);

        drawables = _promptArea.GetDrawables(appState.InputBuffer, appState.CursorIndex);
        _display.Add(drawables);
        _display.Render();
    }

    private void RefreshScreen()
    {
    }

    private void SyncCursor(int cursorIndex, string inputBuffer, int windowHeight)
    {
    }

    private void Render(int windowWidth, int windowHeight)
    {
    }
}
