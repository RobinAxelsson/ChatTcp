using System.Text;
using ChatTcp.Cli;
using ChatTcp.Cli.Shell;
using ChatTcp.Kernel;

internal class ChatScreen
{
    private readonly ConsoleWriter _consoleWriter;
    private readonly Prompt _prompt;
    private readonly StringBuilder _stringBuffer = new();
    public Action<ChatMessageDto> SendChatMessage => _sendChatMessage;
    public Func<JoinChatDto, string>? SendTokenRequest { get; set; }
    private readonly string _alias = "Me";
    private Action<ChatMessageDto> _sendChatMessage;

    private int LineCount
    {
        get
        {
            int lineCount = 1;
            foreach (var chunk in _stringBuffer.GetChunks())
            {
                foreach (var c in chunk.Span)
                {
                    if (c == '\n') lineCount++;
                }
            }

            return lineCount;
        }
    }

    public ChatScreen(Action<Task<ChatMessageDto>> sendChatMessage)
    {
        _consoleWriter = ConsoleWriter.Instance;
        _prompt = new Prompt(_consoleWriter);
    }

    public ChatScreen(ConsoleWriter consoleWriter, Prompt prompt, Action<ChatMessageDto> _sendChatMessage)
    {
        _consoleWriter = consoleWriter;
        _prompt = prompt;
    }

    public void AppendChatMessage(ChatMessageDto chatMessage)
    {
        var text = Styles.FormatChatMessage(chatMessage);


        int messageLineCount = 1 + Styles.MESSAGE_BLANK_LINES;
        foreach (var c in chatMessage.Message)
        {
            if (c == '\n') messageLineCount++;
        }

        int newChatEnd = LineCount + messageLineCount;
        if (newChatEnd > _prompt.CurrentLineIndex)
        {
            _prompt.Jump(newChatEnd + Styles.PROMPT_JUMP_SPACING);
        }

        _consoleWriter.WriteText(text, LineCount + Styles.PROMPT_JUMP_SPACING - 1);

        if (_stringBuffer.Length > 0)
        {
            for (int i = 0; i < Styles.MESSAGE_BLANK_LINES; i++)
            {
                _stringBuffer.AppendLine();
            }
        }

        _stringBuffer.Append(text);
    }


    public async Task Start(CancellationToken ct)
    {
        _prompt.Render();

        while (!ct.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(false);

                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Enter)
                {
                    var text = _prompt.Text;

                    if (string.IsNullOrWhiteSpace(text))
                    {
                        continue;
                    }

                    _prompt.ClearInput();

                    var chatMessage = new ChatMessageDto(_alias, _prompt.Text);
                    SendChatMessage(chatMessage);
                    AppendChatMessage(chatMessage);
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    _prompt.AppendChar('\n');
                    _consoleWriter.Write('\n');
                    continue;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    //no intercept no render
                    _prompt.Backspace();
                    continue;
                }

                _prompt.AppendChar(key.KeyChar);
            }

            await Task.Delay(30, ct);
        }
    }
}
