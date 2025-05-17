

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

            var windowWidth = -1;
            var windowHeight = -1;

            while (true)
            {
                if (windowWidth != GetWindowWidth() || windowHeight != GetWindowHeight())
                {
                    RefreshScreen();
                    windowWidth = GetWindowWidth();
                    windowHeight = GetWindowHeight();
                }

                SyncChatMessages(appState.Messages);
                SyncInputBuffer(appState.InputBuffer, windowWidth, windowHeight);
                SyncCursor(appState.CursorIndex, appState.InputBuffer, windowHeight);

                cancellatioToken.ThrowIfCancellationRequested();
                Render(windowWidth, windowHeight);

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
        if (GetWindowHeight() > 0 && GetWindowWidth() > 0)
        {
            try
            {
                Console.Clear();
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

    private void SyncInputBuffer(string inputBuffer, int windowWidth, int windowHeight)
    {
        var prompt = $"{ShellSettings.Prompt} {inputBuffer}";
        var maxWidth = ShellSettings.FrameBufferWidth < (windowWidth - 1) ? ShellSettings.FrameBufferWidth : (windowWidth - 1);

        //just to not break the program on small windows
        if (windowHeight < ShellSettings.PromptHeight)
            return;

        for (int x = 0; x < maxWidth; x++)
        {
            var y = windowHeight - ShellSettings.PromptHeight;
            if (x < prompt.Length)
            {
                _frameBuffer[x, y] = prompt[x];
                continue;
            }

            //flush deleted chars
            _frameBuffer[x, y] = ' ';
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

    private void Render(int windowWidth, int windowHeight)
    {
        for (int x = 0; x < windowWidth; x++)
        {

            for (int y = 0; y < windowHeight; y++)
            {
                if (_frameBuffer[x, y] != _currentBuffer[x, y])
                {
                    try
                    {
                        Console.SetCursorPosition(x, y);

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
                    }
                    catch (IOException ex)
                    {
                        if (!ex.Message.Contains("The parameter is incorrect"))
                        {
                            throw;
                        }
                        }
                    catch (ArgumentOutOfRangeException)
                        {
                        //Allow the console window to be 0
                        }
                    catch (Exception ex) {
                        var currentChar = _currentBuffer[x, y];
                        var frameChar = _frameBuffer[x, y];
                        throw new ShellException($"Exception when trying to draw to console... Console.WindowWidth {Console.WindowWidth}, x = '{x}', Console.WindowHeight: '{Console.WindowHeight}', y: '{y}', currentChar: '{(currentChar == '\0' ? "null" : currentChar)}', frameChar: '{(frameChar == '\0' ? "null" : frameChar)}'", ex);
                    }
                }
                _currentBuffer[x, y] = _frameBuffer[x, y];
            }

        }
    }
}

