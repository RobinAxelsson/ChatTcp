using System.Text;

namespace ChatTcp.Cli.Shell;

internal class Prompt
{
    private const string PROMPT_PREFIX = "Chat>";
    private readonly ConsoleWriter _consoleWriter;
    private readonly StringBuilder _stringBuffer = new();
    private int _currentLineIndex;

    public Prompt(ConsoleWriter consoleWriter)
    {
        _consoleWriter = consoleWriter;
        _stringBuffer.Append(PROMPT_PREFIX);
    }

    public int LineCount
    {
        get
        {
            int n = 1;
            foreach (var chunk in _stringBuffer.GetChunks())
                foreach (var c in chunk.Span)
                    if (c == '\n') n++;
            return n;
        }
    }

    public int CurrentLineIndex
    {
        get => _currentLineIndex;
        set => _currentLineIndex = value;
    }

    public string Text =>
        _stringBuffer.Length > PROMPT_PREFIX.Length
            ? _stringBuffer.ToString(PROMPT_PREFIX.Length, _stringBuffer.Length - PROMPT_PREFIX.Length)
            : string.Empty;

    public void Jump(int steps)
    {
        _consoleWriter.ClearLines(CurrentLineIndex, LineCount);
        var newIndex = CurrentLineIndex + steps;
        _consoleWriter.WriteText(_stringBuffer.ToString(), newIndex);
        CurrentLineIndex = newIndex;
    }

    public void AppendChar(char ch) => _stringBuffer.Append(ch);

    public void Backspace()
    {
        if (_stringBuffer.Length > PROMPT_PREFIX.Length)
            _stringBuffer.Length -= 1;
    }

    public void Clear()
    {
        _stringBuffer.Length = PROMPT_PREFIX.Length;
    }

    public void Hide() => _consoleWriter.ClearLines(CurrentLineIndex, LineCount);

    public void Render() => _consoleWriter.WriteText(_stringBuffer.ToString(), CurrentLineIndex);

    public override string ToString() => _stringBuffer.ToString();
}
