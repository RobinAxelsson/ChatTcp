using ChatTcp.Cli.Shell.View;

namespace ChatTcp.Cli.Shell;

internal class ConsoleOut
{
    private ChatWindow? _chatWindow;
    private Display? _display;

    private int _windowWidth;
    private int _windowHeight;
    public void Render(AppState appState)
    {
        bool isDefault = _windowHeight == default || _windowWidth == default;
        bool newSize = (_windowHeight != appState.WindowHeight || _windowWidth != appState.WindowWidth);

        if (_chatWindow != null && newSize)
        {
            //clear area
        }

        if (_chatWindow == null)
        {
            _chatWindow = new ChatWindow(0, 0, appState.WindowWidth, appState.WindowHeight);
        }
        else
        {
            _chatWindow.X2 = appState.WindowWidth;
            _chatWindow.Y2 = appState.WindowHeight;
        }

        var drawables = _chatWindow.GetDrawables(appState.Messages);
        

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
