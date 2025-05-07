

namespace ChatTcp.Cli.ConsoleUi;

internal static class ShellSettings
{
    public const string Prompt = "chat>";
    public const int PromptHeight = 3;
    public const int MessageSpacing = 1;
    public const int FrameBufferWidth = 1000;
    public const int FrameBufferHeight = 500;
}

internal class Renderer
{

    private char[,] _currentBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];
    private char[,] _frameBuffer = new char[ShellSettings.FrameBufferWidth, ShellSettings.FrameBufferHeight];

    public void RenderApp(AppState appState)
    {
        AddChatMessagesToFrameBuffer(appState.Messages);
        AddInputBufferToFrame(appState.InputBuffer, appState.WindowHeight);
        AdjustCursor(appState.PromptingMode, appState.WindowHeight, appState.InputBuffer);
        Render();
    }

    private void AdjustCursor(bool promptActive, int windowHeight, string inputBuffer)
    {
        if (promptActive && Console.GetCursorPosition().Top < windowHeight - ShellSettings.PromptHeight)
        {
            Console.SetCursorPosition(ShellSettings.Prompt.Length + inputBuffer.Length + 1, windowHeight - ShellSettings.PromptHeight);
        }
    }

    private void AddInputBufferToFrame(string inputBuffer, int windowHeight)
    {
        var prompt = $"{ShellSettings.Prompt} {inputBuffer}";
        for (int x = 0; x < prompt.Length; x++)
        {
            _frameBuffer[x, windowHeight - ShellSettings.PromptHeight] = prompt[x];
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
                    Console.Write(_frameBuffer[x, y]);
                    _currentBuffer[x, y] = _frameBuffer[x, y];
                }
            }
        }

        Console.SetCursorPosition(curserPosition.Left, curserPosition.Top);
    }
}

