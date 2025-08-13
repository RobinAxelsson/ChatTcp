using System.Text;
using Xunit;

namespace ChatTcp.Cli.Tests
{
    public class RenderSystemHelpersTests
    {
        [Fact]
        public void AppendRowsToStringBuilder_NoOp_When_Rows_IsNull()
        {
            List<List<char>> rows = null!;
            var sb = new StringBuilder("prefix");

            RenderSystemHelpers.AppendRowsToStringBuilder(rows, sb);

            Assert.Equal("prefix", sb.ToString());
        }

        [Fact]
        public void AppendRowsToStringBuilder_NoOp_When_Rows_IsEmpty()
        {
            var rows = new List<List<char>>();
            var sb = new StringBuilder("prefix");

            RenderSystemHelpers.AppendRowsToStringBuilder(rows, sb);

            Assert.Equal("prefix", sb.ToString());
        }

        [Fact]
        public void AppendRowsToStringBuilder_Appends_SingleRow_SkipsEmptyCells_NoTrailingNewline()
        {
            var rows = new List<List<char>>
            {
                new() { 'H', 'i', '!', }
            };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendRowsToStringBuilder(rows, sb);

            Assert.Equal("Hi!", sb.ToString());
        }

        [Fact]
        public void AppendRowsToStringBuilder_Appends_MultipleRows_With_Newlines_BetweenOnly()
        {
            var rows = new List<List<char>>
            {
                new() { 'A', 'B' },
                new() { 'C' },
                new() { 'D' },
                new(),
                null!,
                new() { 'E' }
            };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendRowsToStringBuilder(rows, sb);

            var expected = string.Join(Environment.NewLine, "AB", "C", "D", "", "", "E");
            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void AppendRowsToStringBuilder_Preserves_ExistingContent()
        {
            var rows = new List<List<char>> { new() { 'X' } };
            var sb = new StringBuilder("prefix");

            RenderSystemHelpers.AppendRowsToStringBuilder(rows, sb);

            Assert.Equal("prefixX", sb.ToString());
        }

        [Fact]
        public void AppendRowsToStringBuilder_LastRow_Empty_Results_In_TrailingSeparatorOnly()
        {
            var rows = new List<List<char>>
            {
                new() { 'A' },
                new()
            };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendRowsToStringBuilder(rows, sb);

            Assert.Equal("A" + Environment.NewLine, sb.ToString());
        }

        [Fact]
        public void AppendRowsToStringBuilder_NullRow_Creates_BlankLine_If_Not_Last()
        {
            var rows = new List<List<char>>
            {
                new() { '1' },
                null!,
                new() { '2' }
            };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendRowsToStringBuilder(rows, sb);

            var expected = string.Join(Environment.NewLine, "1", "", "2");
            Assert.Equal(expected, sb.ToString());
        }

        // --- TryGetNextCharAtIndex tests ---
        [Fact]
        public void TryGetNextCharAtIndex_EmptyOldAndNew_ReturnsFalseAndDefault()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: new List<char>(),
                newLine: new List<char>(),
                i: 0,
                out var c);

            Assert.False(ok);
            Assert.Equal('\0', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_OldEmpty_NewHasCharAtIndex_ReturnsTrueAndNewChar()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: new List<char>(),
                newLine: "ABC".ToList(),
                i: 1,
                out var c);

            Assert.True(ok);
            Assert.Equal('B', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_BothHaveCharAtIndex_ReturnsTrueAndNewChar()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "xyz".ToList(),
                newLine: "ABC".ToList(),
                i: 2,
                out var c);

            Assert.True(ok);
            Assert.Equal('C', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_NewTooShort_OldHasCharAtIndex_ReturnsTrueAndSpace()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "HELLO".ToList(),
                newLine: "HE".ToList(),
                i: 3,
                out var c);

            Assert.True(ok);
            Assert.Equal(' ', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_BothTooShortAtIndex_ReturnsFalseAndDefault()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "AB".ToList(),
                newLine: "CD".ToList(),
                i: 5,
                out var c);

            Assert.False(ok);
            Assert.Equal('\0', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_IndexEqualsNewLength_OldHasChar_ReturnsTrueAndSpace()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "ABCDE".ToList(),
                newLine: "XYZ".ToList(),
                i: 3,
                out var c);

            Assert.True(ok);
            Assert.Equal(' ', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_IndexEqualsNewLength_NoOldChar_ReturnsFalseAndDefault()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "XY".ToList(),
                newLine: "AB".ToList(),
                i: 2,
                out var c);

            Assert.False(ok);
            Assert.Equal('\0', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_OldNull_NewHasChar_ReturnsTrueAndNewChar()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: null,
                newLine: "Hi".ToList(),
                i: 1,
                out var c);

            Assert.True(ok);
            Assert.Equal('i', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_OldNull_BothTooShort_ReturnsFalseAndDefault()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: null,
                newLine: "Q".ToList(),
                i: 5,
                out var c);

            Assert.False(ok);
            Assert.Equal('\0', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_NewEmpty_OldHasChar_ReturnsTrueAndSpace()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "OLD".ToList(),
                newLine: new List<char>(),
                i: 1,
                out var c);

            Assert.True(ok);
            Assert.Equal(' ', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_NewHasCharAtZero_ReturnsTrueAndThatChar()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "Z".ToList(),
                newLine: "A".ToList(),
                i: 0,
                out var c);

            Assert.True(ok);
            Assert.Equal('A', c);
        }

        [Fact]
        public void TryGetNextCharAtIndex_NewNull_OldHasChar_ReturnsTrueAndSpace()
        {
            var ok = RenderSystemHelpers.TryGetNextCharAtIndex(
                oldLine: "PQ".ToList(),
                newLine: null,
                i: 1,
                out var c);

            Assert.True(ok);
            Assert.Equal(' ', c);
        }

        [Fact]
        public void AppendOrOverwriteRowsToStringBuilder_Throws_When_StringBuilder_IsNull()
        {
            var toErase = new List<List<char>> { "A".ToList() };
            StringBuilder sb = null!;

            Assert.Throws<ArgumentNullException>(() =>
                RenderSystemHelpers.AppendOrOverwriteRowsToStringBuilder(
                    toErase,
                    toAppend: new List<List<char>>(),
                    stringBuilder: sb));
        }

        [Fact]
        public void AppendOrOverwriteRowsToStringBuilder_Writes_NewChars_When_NewLonger_Than_Old()
        {
            var toErase = new List<List<char>> { "AB".ToList() };
            var toAppend = new List<List<char>> { "ABCDE".ToList() };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendOrOverwriteRowsToStringBuilder(toErase, toAppend, sb);

            Assert.Equal("ABCDE", sb.ToString());
        }

        [Fact]
        public void AppendOrOverwriteRowsToStringBuilder_Erases_ExtraOldChars_With_Spaces()
        {
            var toErase = new List<List<char>> { "ABCDE".ToList() };
            var toAppend = new List<List<char>> { "AB".ToList() };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendOrOverwriteRowsToStringBuilder(toErase, toAppend, sb);

            Assert.Equal("AB   ", sb.ToString());
        }

        [Fact]
        public void AppendOrOverwriteRowsToStringBuilder_MultipleRows_NewlinesBetween_NoTrailing()
        {
            var toErase = new List<List<char>>
            {
                "OLD".ToList(),
                "Y".ToList()
            };
            var toAppend = new List<List<char>>
            {
                "NEW".ToList(),
                new List<char>()
            };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendOrOverwriteRowsToStringBuilder(toErase, toAppend, sb);

            var expected = "NEW" + Environment.NewLine + " ";
            Assert.Equal(expected, sb.ToString());
        }

        [Fact]
        public void AppendOrOverwriteRowsToStringBuilder_InnerNullRow_Treated_As_EmptyRow()
        {
            var toErase = new List<List<char>>
            {
                "AB".ToList()
            };
            var toAppend = new List<List<char>>
            {
                null!
            };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendOrOverwriteRowsToStringBuilder(toErase, toAppend, sb);

            Assert.Equal("  ", sb.ToString());
        }

        [Fact]
        public void AppendOrOverwriteRowsToStringBuilder_Prefers_NewChar_When_BothHave_At_Same_Index()
        {
            var toErase = new List<List<char>> { "XYZ".ToList() };
            var toAppend = new List<List<char>> { "AbC".ToList() };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendOrOverwriteRowsToStringBuilder(toErase, toAppend, sb);

            Assert.Equal("AbC", sb.ToString());
        }

        [Fact]
        public void AppendOrOverwriteRowsToStringBuilder_Handles_Different_Row_Counts()
        {
            var toErase = new List<List<char>>
            {
                "AAA".ToList(),
                "BBBB".ToList()
            };
            var toAppend = new List<List<char>>
            {
                "A".ToList()
            };
            var sb = new StringBuilder();

            RenderSystemHelpers.AppendOrOverwriteRowsToStringBuilder(toErase, toAppend, sb);

            var expected = "A  " + Environment.NewLine + "    ";
            Assert.Equal(expected, sb.ToString());
        }
    }
}
