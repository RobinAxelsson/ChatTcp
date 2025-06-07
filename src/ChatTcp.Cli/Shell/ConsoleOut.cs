using ChatTcp.Cli.Shell.View;

namespace ChatTcp.Cli.Shell;

internal class ConsoleOut
{
    private ChatWindow? _chatWindow;
    private Display _display;

    private int _windowWidth;
    private int _windowHeight;

    public ConsoleOut()
    {
        _display = new Display(300, 200); //this is just to make sure we do not get out of bounds
    }

    internal ConsoleOut(Display display)
    {
        _display = display;
    }

    public void Handle(AppState appState)
    {

        _chatWindow = new ChatWindow(0, 0, appState.WindowWidth, appState.WindowHeight);
        //}
        //else
        //{
        //    _chatWindow.X1 = appState.WindowWidth;
        //    _chatWindow.Y1 = appState.WindowHeight;
        //}

        //---------width--------
        //
        //rendering not so often
        //
        //
        //chat>Hello
        //
        //----------------------

        var drawables = _chatWindow.GetDrawables(appState.Messages);
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
