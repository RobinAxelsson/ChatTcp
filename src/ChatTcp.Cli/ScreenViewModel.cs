using System.Collections;
using System.Text;

internal class ScreenViewModel
{
    private readonly List<StringBuilder> _lines = new List<StringBuilder>();
    private readonly IReadOnlyList<string> _readOnlyLines;

    public string Title
    {
        get => _title;
        set { if (!string.Equals(_title, value, StringComparison.Ordinal)) { _title = value; IsDirty = true; } }
    }
    private string _title;

    /// <summary>
    /// Read-only view of the lines. Indexing/enumeration returns the current string for each line.
    /// </summary>
    public IReadOnlyList<string> Lines => _readOnlyLines;

    public ConsoleColor ForegroundColor
    {
        get => _foregroundColor;
        set { if (_foregroundColor != value) { _foregroundColor = value; IsDirty = true; } }
    }
    private ConsoleColor _foregroundColor;

    public int FirstVisibleRow { get; private set; } = 0;
    public int? LastVisibleRow { get; private set; } = null;

    public bool IsDirty { get; private set; }

    public ScreenViewModel(ConsoleColor foregroundColor, string title)
    {
        _foregroundColor = foregroundColor;
        _title = title ?? string.Empty;
        _readOnlyLines = new StringBuilderReadOnlyList(_lines);
    }

    /// <summary>
    /// Appends text to the specified line, creating intermediate lines if needed.
    /// </summary>
    public void AppendText(string text, int line)
    {
        if (line < 0) throw new ArgumentOutOfRangeException(nameof(line));
        if (string.IsNullOrEmpty(text)) return;

        EnsureLineExists(line);
        _lines[line].Append(text);
        IsDirty = true;
    }

    /// <summary>
    /// Appends a single character to the specified line, creating intermediate lines if needed.
    /// </summary>
    public void AppendChar(char c, int line)
    {
        if (line < 0) throw new ArgumentOutOfRangeException(nameof(line));

        EnsureLineExists(line);
        _lines[line].Append(c);
        IsDirty = true;
    }

    /// <summary>
    /// Adds a new line at the end (optionally with initial text) and returns its index.
    /// </summary>
    public int AppendLine(string? text = null)
    {
        var sb = new StringBuilder();
        if (!string.IsNullOrEmpty(text)) sb.Append(text);
        _lines.Add(sb);
        IsDirty = true;
        return _lines.Count - 1;
    }

    /// <summary>
    /// Replace an entire line's contents (creating the line if it doesn't exist yet).
    /// </summary>
    public void SetLine(int line, string? text)
    {
        if (line < 0) throw new ArgumentOutOfRangeException(nameof(line));

        EnsureLineExists(line);
        var sb = _lines[line];
        sb.Clear();
        if (!string.IsNullOrEmpty(text)) sb.Append(text);
        IsDirty = true;
    }

    /// <summary>
    /// Clears all lines.
    /// </summary>
    public void Clear()
    {
        _lines.Clear();
        IsDirty = true;
    }

    /// <summary>
    /// Adjust the visible window. If last is null, visibility extends to the end.
    /// </summary>
    public void SetVisibleWindow(int first, int? last = null)
    {
        if (first < 0) throw new ArgumentOutOfRangeException(nameof(first));
        if (last is < 0) throw new ArgumentOutOfRangeException(nameof(last));

        FirstVisibleRow = first;
        LastVisibleRow = last;
        IsDirty = true;
    }

    /// <summary>
    /// Mark the view as not dirty (e.g., after rendering).
    /// </summary>
    public void AcknowledgeRendered() => IsDirty = false;

    private void EnsureLineExists(int lineIndex)
    {
        while (_lines.Count <= lineIndex)
            _lines.Add(new StringBuilder());
    }

    /// <summary>
    /// Lightweight adapter that exposes the current line contents as a read-only list of strings.
    /// Strings are materialized on access; no external mutation is possible.
    /// </summary>
    private sealed class StringBuilderReadOnlyList : IReadOnlyList<string>
    {
        private readonly List<StringBuilder> _backing;
        public StringBuilderReadOnlyList(List<StringBuilder> backing) => _backing = backing;

        public string this[int index] => _backing[index].ToString();
        public int Count => _backing.Count;

        public IEnumerator<string> GetEnumerator()
        {
            for (int i = 0; i < _backing.Count; i++)
                yield return _backing[i].ToString();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
