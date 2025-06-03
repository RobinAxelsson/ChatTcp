
using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell;

internal class Renderer
{

    private char[,] _currentBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];
    private char[,] _frameBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];
    private readonly ConsoleAdapterBase _console;
    public Renderer() : this (new ConsoleAdapter()){}
    internal Renderer(ConsoleAdapterBase consoleAdapter)
    {
        _console = consoleAdapter;
    }
    public void Handle(AppEvent appEvent)
    {
        switch (appEvent)
        {
            case WindowResizedEvent resized:
                RefreshScreen();
                Render(resized.Width, resized.Height);
                break;

            case BackspaceEvent backspaceEvent:
                break;

            case SendMessageEvent sendMessageEvent:
                //Clear prompt
                break;

            default:
                break;
        }
    }

    private void RefreshScreen()
    {
        if (_console.WindowWidth > 0 && _console.WindowHeight > 0)
        {
            try
            {
                
            }
            catch (IOException ex)
            {
                if (!ex.Message.Contains("The parameter is incorrect"))
                    throw;
            }
        }
        for (int x = 0; x < ShellSettings.FrameBufferWidth; x++)
        {
            for (int y = 0; y < ShellSettings.FrameBufferHeight; y++)
            {
                _currentBuffer[x, y] = '\0';
                _frameBuffer[x, y] = '\0';
            }
        }
    }

    private void SyncCursor(int cursorIndex, string inputBuffer, int windowHeight)
    {
        if (cursorIndex < 0 || cursorIndex > inputBuffer.Length)
        {
            throw new ShellException($"Cursor index must be within the input buffer length or one place outside. CursorIndex: {cursorIndex}, bufferLength: {inputBuffer.Length}, inputBuffer: {inputBuffer}");
        }

        var x = ShellSettings.Prompt.Length + cursorIndex + 1;
        var y = windowHeight < ShellSettings.PromptHeight ? windowHeight : windowHeight - ShellSettings.PromptHeight;

        _frameBuffer[x, y] = ShellSettings.Cursor;
    }

    private void Render(int windowWidth, int windowHeight)
    {
    }
}
