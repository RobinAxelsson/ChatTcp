
namespace ChatTcp.Cli.ConsoleUi;

internal class Renderer
{
    private const int bufferMaxWidth = 1000;
    private const int bufferMaxHeight = 500;
    private char[,] _currentBuffer = new char[bufferMaxWidth, bufferMaxHeight];
    private char[,] _frameBuffer = new char[bufferMaxWidth, bufferMaxHeight];

    public void RenderApp(AppState appState)
    {
        AddChatMessagesToFrameBuffer(appState.Messages);
        AddInputBufferToFrame(appState.InputBuffer, appState.WindowHeight);

        Render();
    }

    private void AddInputBufferToFrame(string inputBuffer, int windowHeight)
    {
        var prompt = $"chat> {inputBuffer}";
        for (int x = 0; x < prompt.Length; x++)
        {
            _frameBuffer[x, windowHeight - 5] = prompt[x];
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
                _frameBuffer[x, y * 2] = c;
            }
        }
    }

    private void Render()
    {
        var curserPosition = Console.GetCursorPosition();

        for (int x = 0; x < bufferMaxWidth; x++)
        {
            for (int y = 0; y < bufferMaxHeight; y++)
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

