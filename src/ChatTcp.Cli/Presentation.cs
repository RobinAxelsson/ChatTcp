using System.Text;

namespace ChatTcp.Cli.Presentation;
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

internal class Prompt
{
    private readonly ConsoleWriter _consoleWriter;
    private readonly StringBuilder _stringBuffer = new();
    private int _currentLineIndex;

    public Prompt(ConsoleWriter consoleWriter)
    {
        _consoleWriter = consoleWriter;
        _stringBuffer.Append(Styles.PROMPT_PREFIX);
    }

    public int LineCount
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

    public int CurrentLineIndex
    {
        get => _currentLineIndex;
        set => _currentLineIndex = value;
    }

    public string Text
    {
        get
        {
            int length = _stringBuffer.Length;
            int promptLength = Styles.PROMPT_PREFIX.Length;
            int textLength = length - promptLength;
            return _stringBuffer.ToString(promptLength, textLength);
        }
    }

    public void Jump(int newIndex)
    {
        _consoleWriter.ClearLines(CurrentLineIndex, LineCount);
        _consoleWriter.WriteText(_stringBuffer.ToString(), newIndex);
        CurrentLineIndex = newIndex;
    }

    public void AppendChar(char ch) => _stringBuffer.Append(ch);

    public void Backspace()
    {
        if (_stringBuffer.Length > Styles.PROMPT_PREFIX.Length)
        {
            _stringBuffer.Length -= 1;
        }
    }

    public void ClearInput()
    {
        _stringBuffer.Length = Styles.PROMPT_PREFIX.Length;
    }

    public void Hide() => _consoleWriter.ClearLines(CurrentLineIndex, LineCount);

    public void Render() => _consoleWriter.WriteText(_stringBuffer.ToString(), CurrentLineIndex);

    public override string ToString() => _stringBuffer.ToString();
}

internal static class Styles
{
    public const int MESSAGE_BLANK_LINES = 2;
    public const int PROMPT_JUMP_SPACING = 8;
    public const string PROMPT_PREFIX = "Chat>";

    public static string FormatChatMessage(ChatMessageDto chatMessage) => $"{chatMessage.Sender}: {chatMessage.Message}";
}

internal class ConsoleWriter
{
    private static ConsoleWriter? _instance;
    private static readonly object _lock = new();
    private readonly IConsoleAdapter _consoleAdapter;
    private StringBuilder _sb = new();
    private ConsoleWriterMemory _consoleLineMemory = new();
    public static ConsoleWriter Instance => _instance ?? (_instance = new ConsoleWriter(ConsoleAdapter.Instance));

    //test constructor
    internal ConsoleWriter(IConsoleAdapter consoleAdapter)
    {
        _consoleAdapter = consoleAdapter;
    }

    private static void ConsoleLock(Action action, IConsoleAdapter consoleAdapter)
    {
        lock (_lock)
        {
            var cursorVisibleStored = Console.CursorVisible;
            Console.CursorVisible = false;

            action();

            Console.CursorVisible = cursorVisibleStored;
        }
    }

    internal void Write(string text)
    {
        LockConsole(() =>
        {
            Console.Write(text);
        });
    }

    internal void Write(char ch)
    {
        LockConsole(() =>
        {
            Console.Write(ch);
        });
    }

    public void WriteText(string text, int lineIndexStart)
    {
        LockConsole(() =>
        {
            _consoleAdapter.SetCursorPosition(0, lineIndexStart);
            _sb.Clear();

            int lineLength = 0;
            char last = default;
            int lineIndex = 0;

            foreach (var ch in text)
            {
                lineLength++;

                if (ch == '\n')
                {
                    lineLength = 0;
                    if (_consoleLineMemory.TryGetLineLength(lineIndex, out int oldLineLength) && oldLineLength > lineLength)
                    {
                        int padding = oldLineLength - lineLength;
                        _sb.Append(new string(' ', padding));
                    }

                    lineIndex++;
                }

                _sb.Append(ch);
            }

            if (last != '\n')
            {
                _sb.Append("buffer-height:");
                _sb.Append(Console.BufferHeight);
                _sb.Append(Environment.NewLine);
            }

            Console.Write(_sb.ToString());
            _consoleLineMemory.UpdateLineLengths(lineIndexStart, text);
        });
    }

    public void WriteText(string text)
    {
        WriteText(text, Console.CursorTop);
    }

    public void Clear()
    {
        LockConsole(() =>
        {
            _consoleAdapter.Clear();
            _consoleLineMemory.Clear();
        });
    }

    public void ClearLines(int start, int lineCount)
    {
        LockConsole(() =>
        {
            int originalLeft = Console.CursorLeft;
            int originalTop = Console.CursorTop;
            _sb.Clear();

            for (int i = 0; i < lineCount; i++)
            {
                if (_consoleLineMemory.TryGetLineLength(i, out int dictLineLength))
                {
                    _sb.Append(new string(' ', dictLineLength));
                }
                else
                {
                    _sb.Append(Environment.NewLine);
                }
            }

            WriteText(_sb.ToString());
            _consoleAdapter.SetCursorPosition(originalLeft, originalTop);
        });
    }

    public void ClearLine(int lineNumber)
    {
        ClearLines(lineNumber, 1);
    }

    private void LockConsole(Action action)
    {
        ConsoleLock(action, _consoleAdapter);
    }
}

internal class ConsoleWriterMemory
{
    private readonly Dictionary<int, int> _lineLengthDict = new();

    internal bool TryGetLineLength(int index, out int length) =>
        _lineLengthDict.TryGetValue(index, out length);

    public int GetLongestLineLength() => _lineLengthDict.Max(x => x.Value);
    public void UpdateLineLengths(int start, string text)
    {
        var lines = text.Replace("\r\n", "\n").Split('\n', StringSplitOptions.None);
        for (int i = 0; i < lines.Length; i++)
        {
            var idx = start + i;
            var len = lines[i].Length;
            _lineLengthDict[idx] = len;
        }
    }

    public void Clear()
    {
        _lineLengthDict.Clear();
    }
}

internal class ConsoleAdapter : IConsoleAdapter
{
    private static ConsoleAdapter? _instance = null;
    private Action<int, int> _setCursorPosition = SetCursorPositionCore;
    private Action _clear = Console.Clear;
    private ConsoleAdapter() { }

    internal static ConsoleAdapter Instance => _instance ??= new ConsoleAdapter();

    //test constructor
    internal ConsoleAdapter(Action<int, int> setCursorPosition, Action clear)
    {
        _setCursorPosition = setCursorPosition;
        _clear = clear;
    }

    //public void SetCursorPosition(int x, int y) => _setCursorPosition?.Invoke(x, y);

    public void SetCursorPosition(int x, int y)
    {
        _setCursorPosition(x, y);
    }

    private static void SetCursorPositionCore(int x, int y)
    {
        if (x < 0 || y < 0) throw new ArgumentOutOfRangeException();

        // Grow height first (must be >= WindowHeight)
        if (y >= Console.BufferHeight)
        {
            int newHeight = Math.Max(y + 9000, Console.WindowHeight);
            Console.BufferHeight = newHeight;
        }
        // Grow width if needed
        if (x >= Console.BufferWidth)
        {
            int newWidth = Math.Max(x + 1, Console.WindowWidth);
            Console.BufferWidth = newWidth;
        }

        Console.SetCursorPosition(x, y);
    }

    public void Clear() => _clear?.Invoke();
}

internal interface IConsoleAdapter
{
    void Clear();
    void SetCursorPosition(int x, int y);
}
