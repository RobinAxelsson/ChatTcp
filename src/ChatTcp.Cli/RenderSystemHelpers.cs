using System.Text;
using ChatTcp.Cli.Shell;

namespace ChatTcp.Cli;

public static class RenderSystemHelpers
{
    internal static void AppendRowsToStringBuilder(List<List<char>> rows, StringBuilder stringBuilder)
    {
        if (stringBuilder == null) throw new ArgumentNullException(nameof(stringBuilder));

        if (rows == null || rows.Count == 0) return;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];

            if (row != null)
            {
                foreach (var cell in row)
                {
                    if (cell != default)
                    {
                        stringBuilder.Append(cell);
                    }
                }
            }

            // Add newline between toErase, but not after the last row
            if (i < rows.Count - 1)
            {
                stringBuilder.Append(Environment.NewLine);
            }
        }
    }

    internal static void AppendOrOverwriteRowsToStringBuilder(
    List<List<char>>? toErase,
    List<List<char>>? toAppend,
    StringBuilder stringBuilder)
    {
        if (stringBuilder is null) throw new ArgumentNullException(nameof(stringBuilder));

        int eraseRows = toErase?.Count ?? 0;
        int appendRows = toAppend?.Count ?? 0;
        int maxRows = Math.Max(eraseRows, appendRows);
        if (maxRows == 0) return;

        for (int r = 0; r < maxRows; r++)
        {
            List<char>? oldRow = (toErase != null && r < eraseRows) ? toErase[r] : null;
            List<char>? newRow = (toAppend != null && r < appendRows) ? toAppend[r] : null;

            int oldLen = oldRow?.Count ?? 0;
            int newLen = newRow?.Count ?? 0;
            int maxCols = Math.Max(oldLen, newLen);

            for (int i = 0; i < maxCols; i++)
            {
                if (TryGetNextCharAtIndex(oldRow, newRow, i, out var c))
                {
                    stringBuilder.Append(c);
                }
            }

            if (r < maxRows - 1)
            {
                stringBuilder.Append(Environment.NewLine);
            }
        }
    }

    internal static bool TryGetNextCharAtIndex(List<char>? oldLine, List<char>? newLine, int i, out char c)
    {
        c = default;

        var newLineCount = newLine?.Count ?? 0;
        var oldLineCount = oldLine?.Count ?? 0;

        //If empty lists
        if (newLineCount == 0 && oldLineCount == 0)
        {
            return false;
        }

        bool newCharExistAtIndex = newLineCount > i;

        if (newCharExistAtIndex) //and if both exist
        {
            c = newLine![i];
            return true;
        }

        bool oldCharExistAtIndex = oldLineCount > i;

        if (oldCharExistAtIndex)
        {
            c = ' ';
            return true;
        }

        if (!oldCharExistAtIndex && !newCharExistAtIndex)
        {
            return false;
        }

        throw new InvalidStateException();
    }
}
