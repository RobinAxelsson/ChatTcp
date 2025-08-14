using System.Text;

namespace ChatTcp.Cli;

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
