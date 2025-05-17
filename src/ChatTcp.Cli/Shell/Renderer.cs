

namespace ChatTcp.Cli.ConsoleUi;

internal class Renderer
{

    private char[,] _currentBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];
    private char[,] _frameBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];

    private Func<int> GetWindowWidth = () => Console.WindowWidth;
    private Func<int> GetWindowHeight = () => Console.WindowHeight;

    public async Task Start(AppState appState, CancellationToken cancellatioToken)
    {
        try
        {
            Console.CursorVisible = false;

            var windowWidth = GetWindowWidth();
            var windowHeight = GetWindowHeight();
            
            while (true)
            {
                if(windowWidth != GetWindowWidth() || windowHeight != GetWindowHeight())
                {
                    RefreshScreen();
                    windowWidth = GetWindowWidth();
                    windowHeight = GetWindowHeight();
                }

                SyncChatMessages(appState.Messages);
                SyncInputBuffer(appState.InputBuffer, windowWidth, windowHeight);
                cancellatioToken.ThrowIfCancellationRequested();
                SyncCursor(appState.CursorIndex, appState.InputBuffer, windowHeight);

                Render();

                await Task.Delay(ShellSettings.RefreshRate, cancellatioToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Rendering canceled...");
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }

    private void RefreshScreen()
    {
        if(GetWindowHeight() > 0 && GetWindowWidth() > 0)
        {
            Console.Clear();
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
            throw new ArgumentOutOfRangeException($"Cursor index must be within the input buffer length or one place outside. CursorIndex: {cursorIndex}, bufferLength: {inputBuffer.Length}, inputBuffer: {inputBuffer}");
        }

        var x = ShellSettings.Prompt.Length + cursorIndex + 1;
        var y = windowHeight < ShellSettings.PromptHeight ? windowHeight : windowHeight - ShellSettings.PromptHeight;

        _frameBuffer[x, y] = ShellSettings.Cursor;
    }

    private void SyncInputBuffer(string inputBuffer, int windowWidth, int windowHeight)
    {
        var prompt = $"{ShellSettings.Prompt} {inputBuffer}";
        var maxWidth = ShellSettings.FrameBufferWidth < (windowWidth - 1) ? ShellSettings.FrameBufferWidth : (windowWidth - 1);

        if (windowHeight < ShellSettings.PromptHeight) return;

        for (int x = 0; x < maxWidth; x++)
        {
            if (x < prompt.Length)
            {
                _frameBuffer[x, windowHeight - ShellSettings.PromptHeight] = prompt[x];
                continue;
            }

            //flush deleted chars
            _frameBuffer[x, windowHeight - ShellSettings.PromptHeight] = ' ';
        }
    }

    private void SyncChatMessages(List<Message> messages)
    {
        bool previousMessageIsCurrentUser = false;
        int previousY = -1;

        for (int i = 0; i < messages.Count; i++)
        {
            var message = messages[i];

            var chatMessage = message.IsCurrentUser ? new String('\0', ShellSettings.CurrentUserMessageIndentation) + message.Content : $"{message.Sender}: {message.Content}";

            int y = -1;
            //To enable current user messages to appear underneath each other
            if (previousY > -1 && previousMessageIsCurrentUser && message.IsCurrentUser)
            {
                y = previousY + 1;
            }
            //Message spacing on incoming messages
            else
            {
                y = i * (ShellSettings.MessageSpacing + 1);
            }

            for (int x = 0; x < chatMessage.Length; x++)
            {
                var c = chatMessage[x];
                _frameBuffer[x, y] = c;
            }

            previousMessageIsCurrentUser = message.IsCurrentUser;
            previousY = y;
        }
    }

    private void Render()
    {
        for (int x = 0; x < ShellSettings.FrameBufferWidth; x++)
        {

            for (int y = 0; y < ShellSettings.FrameBufferHeight; y++)
            {
                if (x >= (GetWindowWidth()-1)) break; //always check the console width before drawing
                if (y >= (GetWindowHeight()-1)) break;

                if (_frameBuffer[x, y] != _currentBuffer[x, y])
                {
                    try
                    {
                        Console.SetCursorPosition(x, y);
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        var currentChar = _currentBuffer[x, y];
                        var frameChar = _frameBuffer[x, y];
                        throw new ShellException($"ArgumentOutOfRangeException when trying to set the cursor position, Console.WindowWidth {Console.WindowWidth}, x = '{x}', Console.WindowHeight: '{Console.WindowHeight}', y: '{y}', currentChar: '{(currentChar == '\0' ? "null" : currentChar)}', frameChar: '{(frameChar == '\0' ? "null" : frameChar)}'", ex);
                    }

                    if (_frameBuffer[x, y] == '\0')
                    {
                        Console.Write(' ');
                        Console.CursorLeft--;
                        continue;
                    }
                    else
                    {
                        Console.Write(_frameBuffer[x, y]);
                    }

                    _currentBuffer[x, y] = _frameBuffer[x, y];
                }
            }
        }
    }
}

