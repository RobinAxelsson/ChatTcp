using ChatTcp.Cli.Shell.View;

namespace ChatTcp.Cli.Shell;

internal class ConsoleOutputManager
{
    private ChatArea? _chatWindow;
    private Display _display;
    private PromptArea? _promptArea;
    //private AppState? _lastAppState;
    public ConsoleOutputManager()
    {
        _display = new Display(300, 300); //this is just to make sure we do not get out of bounds
    }

    internal ConsoleOutputManager(Display display)
    {
        _display = display;
    }

    public void Handle(AppState appState)
    {
        if(_chatWindow == null)
        {
            _chatWindow = new ChatArea(0, 0, appState.WindowWidth, appState.WindowHeight-10);
        }
        else
        {
            _chatWindow.X1 = appState.WindowWidth;
            _chatWindow.Y1 = appState.WindowHeight-10;
        }

        if (_promptArea == null)
        {
            _promptArea = new PromptArea(0, appState.WindowHeight - 8, appState.WindowWidth, appState.WindowHeight);
        }
        else
        {
            _promptArea.Y0 = appState.WindowHeight - 8;
            _promptArea.X1 = appState.WindowWidth;
            _promptArea.Y1 = appState.WindowHeight;
        }

        //var test = "test";
        //if(test == "false")
        //{
        //    _display.ClearArea(_chatWindow.X0, _chatWindow.Y0, _chatWindow.X1, _chatWindow.Y1);
        //}

        _display.ClearArea(_chatWindow.X0, _chatWindow.Y0, _chatWindow.X1, _chatWindow.Y1);
        var drawables = _chatWindow.GetDrawables(appState.Messages);
        _display.Add(drawables);

        drawables = _promptArea.GetDrawables(appState.InputBuffer);

        _display.Add(drawables);

        _display.Render();
    }

    private void SyncCursor()
    {

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
