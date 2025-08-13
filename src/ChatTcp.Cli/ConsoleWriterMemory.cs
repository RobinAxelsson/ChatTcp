namespace ChatTcp.Cli;

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
