using System.Text;

namespace ChatTcp.Cli;

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
