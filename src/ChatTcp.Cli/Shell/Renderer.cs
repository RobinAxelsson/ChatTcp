

namespace ChatTcp.Cli.ConsoleUi;

internal class Renderer
{

    private char[,] _currentBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];
    private char[,] _frameBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];

    public async Task Start(AppState appState, CancellationToken cancellatioToken)
    {
        Console.CursorVisible = false;
        try
        {
            while (true)
            {
                SyncChatMessages(appState.Messages);
                SyncInputBuffer(appState.InputBuffer, appState.WindowHeight);
                AdjustCursor(appState.PromptingMode, appState.WindowHeight, appState.InputBuffer);
                cancellatioToken.ThrowIfCancellationRequested();

                Render();

                await Task.Delay(ShellSettings.RefreshRate, cancellatioToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Rendering canceled...");
        }
    }

    private void AdjustCursor(bool promptActive, int windowHeight, string inputBuffer)
    {
        if (promptActive && Console.GetCursorPosition().Top < windowHeight - ShellSettings.PromptHeight)
        {
            Console.SetCursorPosition(ShellSettings.Prompt.Length + inputBuffer.Length + 1, windowHeight - ShellSettings.PromptHeight);
        }
    }

    private void SyncInputBuffer(string inputBuffer, int windowHeight)
    {
        var prompt = $"{ShellSettings.Prompt} {inputBuffer}";
        for (int x = 0; x < ShellSettings.FrameBufferWidth; x++)
        {
            if (x < prompt.Length)
            {
                _frameBuffer[x, windowHeight - ShellSettings.PromptHeight] = prompt[x];
                continue;
            }

            //flush deleted chars
            _frameBuffer[x, windowHeight - ShellSettings.PromptHeight] = '\0';
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
        var curserPosition = Console.GetCursorPosition();

        for (int x = 0; x < ShellSettings.FrameBufferWidth; x++)
        {
            for (int y = 0; y < ShellSettings.FrameBufferHeight; y++)
            {
                if (_frameBuffer[x, y] != _currentBuffer[x, y])
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

                    _currentBuffer[x, y] = _frameBuffer[x, y];
                }
            }
        }

        Console.SetCursorPosition(curserPosition.Left, curserPosition.Top);
    }
}

