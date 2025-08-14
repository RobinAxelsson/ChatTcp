using System.Collections.Concurrent;
using System.Text;
using ChatTcp.Cli;
using ChatTcp.Cli.Shell;
using ChatTcp.Kernel;

internal class Chat
{
    private readonly ConsoleWriter _consoleWriter;
    private readonly Prompt _prompt;
    private readonly StringBuilder _stringBuffer = new();

    private readonly ConcurrentQueue<ChatMessageDto> _incomingChatMessageQueue = new();
    private int _nextChatRow;

    public Action<ChatMessageDto>? SendChatMessage { get; set; }
    public Func<JoinChatDto, string>? SendTokenRequest { get; set; }
    private readonly string _alias = "Me";

    public Chat() : this(ConsoleWriter.Instance, new Prompt(ConsoleWriter.Instance)) { }

    public Chat(ConsoleWriter consoleWriter, Prompt prompt)
    {
        _consoleWriter = consoleWriter;
        _prompt = prompt;
    }

    public void ReceiveServerPacket(WirePacketDto dto)
    {
        switch (dto)
        {
            case ChatMessageDto m:
                _incomingChatMessageQueue.Enqueue(m);
                break;
            default:
                throw new ShellException("Not implemented packet: " + dto);
        }
    }

    public async Task Start(CancellationToken ct)
    {
        _nextChatRow = 0;
        _prompt.CurrentLineIndex = 10;
        _prompt.Render();

        while (!ct.IsCancellationRequested)
        {
            if (_incomingChatMessageQueue.TryDequeue(out var chatMessage))
            {
                var chatEntry = $"{chatMessage.Sender}: {chatMessage.Message}";
                AppendLine(chatMessage.ToString());

                if (_nextChatRow + CountLines(chatEntry) >= _prompt.CurrentLineIndex)
                {
                    _prompt.Hide();
                    _prompt.Jump((_nextChatRow + CountLines(chatEntry) + 1) - _prompt.CurrentLineIndex);
                    _prompt.Render();
                }

                _consoleWriter.WriteText(chatEntry, _nextChatRow);
                _nextChatRow += CountLines(chatEntry);
                await Task.Delay(1);
            }

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);

                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Enter)
                {
                    var text = _prompt.Text.TrimEnd();
                    if (string.IsNullOrWhiteSpace(text))
                        continue;

                    _prompt.Hide();
                    AppendLine($"{_alias}: {text}");
                    _consoleWriter.WriteText($"{_alias}: {text}", _nextChatRow);

                    SendChatMessage?.Invoke(new ChatMessageDto(_alias, text));

                    _nextChatRow += CountLines(text) + 1;
                    _prompt.Clear();

                    if (_nextChatRow >= _prompt.CurrentLineIndex)
                        _prompt.Jump((_nextChatRow + 1) - _prompt.CurrentLineIndex);

                    _prompt.Render();
                    await Task.Delay(1);
                    continue;
                }

                if (key.Key == ConsoleKey.Enter)
                {
                    _prompt.AppendChar('\n');
                    _prompt.Render();
                    continue;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    _prompt.Backspace();
                    _prompt.Render();
                    continue;
                }

                _prompt.AppendChar(key.KeyChar);
                _prompt.Render();
            }
        }

        _consoleWriter.WriteText(nameof(Chat) + " exited gracefully", _nextChatRow);
    }

    public void AppendLine(string text)
    {
        if (_stringBuffer.Length > 0)
            _stringBuffer.AppendLine();
        _stringBuffer.Append(text);
    }

    public string FullChatText => _stringBuffer.ToString();

    private static int CountLines(string s)
    {
        int n = 1;
        foreach (var ch in s)
            if (ch == '\n') n++;
        return n;
    }
}
