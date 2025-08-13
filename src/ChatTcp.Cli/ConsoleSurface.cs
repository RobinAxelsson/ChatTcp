using System.Runtime.InteropServices;

public sealed class ConsoleSurface
{
    // ----- Public API -----

    /// <summary>Draw a string at (left, top) with optional colors (managed only).</summary>
    public void WriteString(int left, int top, string text,
                            ConsoleColor? fg = null, ConsoleColor? bg = null, bool leaveColors = false)
    {
        if (text is null) text = string.Empty;
        var (oldX, oldY) = (Console.CursorLeft, Console.CursorTop);
        var (oldFg, oldBg) = (Console.ForegroundColor, Console.BackgroundColor);

        if (fg.HasValue) Console.ForegroundColor = fg.Value;
        if (bg.HasValue) Console.BackgroundColor = bg.Value;

        Console.SetCursorPosition(left, top);
        Console.Write(text);

        if (!leaveColors) { Console.ForegroundColor = oldFg; Console.BackgroundColor = oldBg; }
        Console.SetCursorPosition(oldX, oldY);

        if (MirrorWrites) MirrorWrite(left, top, text, fg ?? oldFg, bg ?? oldBg);
    }

    /// <summary>Draw a rectangle by writing the provided lines (managed only).</summary>
    public void WriteRect(int left, int top, string[] lines,
                          ConsoleColor? fg = null, ConsoleColor? bg = null, bool leaveColors = false)
    {
        if (lines == null || lines.Length == 0) return;
        var (oldX, oldY) = (Console.CursorLeft, Console.CursorTop);
        var (oldFg, oldBg) = (Console.ForegroundColor, Console.BackgroundColor);

        if (fg.HasValue) Console.ForegroundColor = fg.Value;
        if (bg.HasValue) Console.BackgroundColor = bg.Value;

        for (int i = 0; i < lines.Length; i++)
        {
            var s = lines[i] ?? string.Empty;
            Console.SetCursorPosition(left, top + i);
            Console.Write(s);

            if (MirrorWrites)
                MirrorWrite(left, top + i, s, fg ?? oldFg, bg ?? oldBg);
        }

        if (!leaveColors) { Console.ForegroundColor = oldFg; Console.BackgroundColor = oldBg; }
        Console.SetCursorPosition(oldX, oldY);
    }

    /// <summary>Fast-ish fill via managed Console writes (no interop).</summary>
    public void FillRect(int left, int top, int width, int height, char ch = ' ',
                         ConsoleColor? fg = null, ConsoleColor? bg = null, bool leaveColors = false)
    {
        var line = new string(ch, Math.Max(0, width));
        var arr = new string[Math.Max(0, height)];
        for (int i = 0; i < arr.Length; i++) arr[i] = line;
        WriteRect(left, top, arr, fg, bg, leaveColors);
    }

    /// <summary>
    /// Peek a rectangle. If preferNative==true (default) and on Windows with a real console,
    /// uses a SINGLE Win32 call (ReadConsoleOutput). Otherwise falls back to the managed mirror.
    /// </summary>
    public Cell[,] PeekRect(int left, int top, int width, int height, bool preferNative = true)
    {
        if (preferNative && CanUseNativePeek() && TryNativePeek(left, top, width, height, out var cells))
            return cells;

        // Managed mirror fallback:
        var result = new Cell[height, width];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                result[y, x] = _mirror.TryGetValue((left + x, top + y), out var c)
                    ? c
                    : new Cell(' ', Console.ForegroundColor, Console.BackgroundColor);
        return result;
    }

    /// <summary>Ensure the console buffer has at least this size (managed only).</summary>
    public void EnsureBufferSize(int width, int height)
    {
        width = Math.Max(width, Console.BufferWidth);
        height = Math.Max(height, Console.BufferHeight);
        Console.SetBufferSize(width, height);
    }

    /// <summary>Keep an internal managed mirror of what this class draws.</summary>
    public bool MirrorWrites { get; set; } = true;

    // ----- Implementation (managed) -----

    private readonly Dictionary<(int x, int y), Cell> _mirror = new();

    private void MirrorWrite(int left, int top, string text, ConsoleColor fg, ConsoleColor bg)
    {
        for (int i = 0; i < text.Length; i++)
            _mirror[(left + i, top)] = new Cell(text[i], fg, bg);
    }

    // ----- Implementation (Windows-only peek) -----

    private static bool CanUseNativePeek()
    {
        if (!OperatingSystem.IsWindows()) return false;
        if (Console.IsOutputRedirected) return false;
        return true;
    }

    private bool TryNativePeek(int left, int top, int width, int height, out Cell[,] rect)
    {
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindows()) { rect = default!; return false; }
#else
        // On older TFMs you could check Environment.OSVersion.Platform instead.
#endif
        rect = new Cell[height, width];

        IntPtr hOut = GetStdHandle(STD_OUTPUT_HANDLE);
        if (hOut == IntPtr.Zero || hOut == INVALID_HANDLE_VALUE) return false;

        var bufferSize = new COORD { X = (short)width, Y = (short)height };
        var buffer = new CHAR_INFO[width * height];

        var region = new SMALL_RECT
        {
            Left = (short)left,
            Top = (short)top,
            Right = (short)(left + width - 1),
            Bottom = (short)(top + height - 1)
        };

        bool ok = ReadConsoleOutput(hOut, buffer, bufferSize, new COORD { X = 0, Y = 0 }, ref region);
        if (!ok) return false;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                var ci = buffer[y * width + x];
                rect[y, x] = new Cell(
                    ci.UnicodeChar,
                    (ConsoleColor)(ci.Attributes & 0x0F),
                    (ConsoleColor)((ci.Attributes >> 4) & 0x0F));
            }
        return true;
    }

    // ----- P/Invoke (only used by TryNativePeek) -----

    private const int STD_OUTPUT_HANDLE = -11;
    private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

    [StructLayout(LayoutKind.Sequential)]
    private struct COORD { public short X; public short Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct SMALL_RECT { public short Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    private struct CHAR_INFO
    {
        [FieldOffset(0)] public char UnicodeChar;
        [FieldOffset(2)] public ushort Attributes;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool ReadConsoleOutput(
        IntPtr hConsoleOutput,
        [Out] CHAR_INFO[] lpBuffer,
        COORD dwBufferSize,
        COORD dwBufferCoord,
        ref SMALL_RECT lpReadRegion);

    // ----- Data type you can use -----

    public readonly struct Cell
    {
        public readonly char Char;
        public readonly ConsoleColor Foreground, Background;
        public Cell(char c, ConsoleColor fg, ConsoleColor bg)
        { Char = c; Foreground = fg; Background = bg; }
        public override string ToString() => $"{Char}({Foreground}/{Background})";
    }
}
