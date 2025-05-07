

namespace ChatTcp.Cli.ConsoleUi;

internal class Renderer
{

    private char[,] _currentBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];
    private char[,] _frameBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];

    public async Task Start(AppState appState)
    {
        Console.CursorVisible = false;
        while (true)
        {
            AddChatMessagesToFrameBuffer(appState.Messages);
            SyncInputBufferWithFrame(appState.InputBuffer, appState.WindowHeight);
            AdjustCursor(appState.PromptingMode, appState.WindowHeight, appState.InputBuffer);
            Render();

            await Task.Delay(ShellSettings.RefreshRate);
        }
    }

    private void AdjustCursor(bool promptActive, int windowHeight, string inputBuffer)
    {
        if (promptActive && Console.GetCursorPosition().Top < windowHeight - ShellSettings.PromptHeight)
        {
            Console.SetCursorPosition(ShellSettings.Prompt.Length + inputBuffer.Length + 1, windowHeight - ShellSettings.PromptHeight);
        }
    }

    private void SyncInputBufferWithFrame(string inputBuffer, int windowHeight)
    {
        var prompt = $"{ShellSettings.Prompt} {inputBuffer}";
        for (int x = 0; x < ShellSettings.FrameBufferWidth; x++)
        {
            if(x < prompt.Length)
            {
                _frameBuffer[x, windowHeight - ShellSettings.PromptHeight] = prompt[x];
                continue;
            }

            //flush deleted chars
            _frameBuffer[x, windowHeight - ShellSettings.PromptHeight] = '\0';
        }
    }

    private void AddChatMessagesToFrameBuffer(List<Message> messages)
    {
        var chatMessages =  messages.Select(x => $"{x.Sender}: {x.Content}").ToArray();

        for (int y = 0; y < chatMessages.Count(); y++)
        {
            var message = chatMessages[y];
            for (global::System.Int32 x = 0; x < message.Length; x++)
            {
                var c = message[x];
                _frameBuffer[x, y * (ShellSettings.MessageSpacing + 1)] = c;
            }
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

                    if(_frameBuffer[x, y] == '\0')
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

