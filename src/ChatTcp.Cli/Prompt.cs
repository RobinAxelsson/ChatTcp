using System.Text;
namespace ChatTcp.Cli.Shell;

internal class Prompt
{
    private const string PROMPT_PREFIX = "Chat>";
    private readonly ConsoleWriter _consoleWriter;
    private StringBuilder _stringBuffer;
    private int _currentLineIndex;

    public Prompt(ConsoleWriter consoleWriter)
    {
        _consoleWriter = consoleWriter;
        _stringBuffer = new StringBuilder();
        _stringBuffer.Append(PROMPT_PREFIX);
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
                    if (c == '\n')
                    {
                        lineCount++;
                    }
                }
            }
            return lineCount;
        }
    }

    public int CurrentLineIndex
    {
        get { return _currentLineIndex; }
        set { _currentLineIndex = value; }
    }

    public void Jump(int steps)
    {
        _consoleWriter.ClearLines(CurrentLineIndex, LineCount);

        int newLineIndex = CurrentLineIndex + steps;
        _consoleWriter.WriteText(_stringBuffer.ToString(), newLineIndex);

        CurrentLineIndex = newLineIndex;
    }

    public void AppendChar(char ch)
    {
        _stringBuffer.Append(ch);
    }

    public void Hide()
    {
        _consoleWriter.ClearLines(CurrentLineIndex, LineCount);
    }

    public void Render()
    {
        _consoleWriter.WriteText(_stringBuffer.ToString(), CurrentLineIndex);
    }

    public override string ToString() => "prompt: '"+ _stringBuffer.ToString() + "'";
}
