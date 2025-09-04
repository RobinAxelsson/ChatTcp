using ChatTcp.Cli.Presentation;
using Xunit;

namespace ChatTcp.Cli.Tests;

public class ConsoleLineMemoryTests
{
    private ConsoleWriterMemory CreateSut() => new ConsoleWriterMemory();

    [Fact]
    public void UpdateLineLengths_MultiLine_StoresSequentialIndices()
    {
        var m = CreateSut();
        m.UpdateLineLengths(2, "aa\nbbbb\nc");
        Assert.True(m.TryGetLineLength(2, out var l0));
        Assert.Equal(2, l0);

        Assert.True(m.TryGetLineLength(3, out var l1));
        Assert.Equal(4, l1);

        Assert.True(m.TryGetLineLength(4, out var l2));
        Assert.Equal(1, l2);
    }

    [Fact]
    public void UpdateLineLengths_OverwritesExisting()
    {
        var m = CreateSut();
        m.UpdateLineLengths(0, "123456");
        Assert.True(m.TryGetLineLength(0, out var l0));
        Assert.Equal(6, l0);

        m.UpdateLineLengths(0, "12");
        Assert.True(m.TryGetLineLength(0, out l0));
        Assert.Equal(2, l0);
    }

    [Fact]
    public void UpdateLineLengths_HandlesCrLf()
    {
        var m = CreateSut();
        m.UpdateLineLengths(1, "a\r\nbb\r\nccc");
        Assert.True(m.TryGetLineLength(1, out var l1));
        Assert.True(m.TryGetLineLength(2, out var l2));
        Assert.True(m.TryGetLineLength(3, out var l3));
        Assert.Equal(1, l1);
        Assert.Equal(2, l2);
        Assert.Equal(3, l3);
    }

    [Fact]
    public void Clear_RemovesAll()
    {
        var m = CreateSut();
        m.UpdateLineLengths(0, "x\ny");
        m.Clear();
        Assert.False(m.TryGetLineLength(0, out _));
        Assert.False(m.TryGetLineLength(1, out _));
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("a", 1)]
    [InlineData("ab", 2)]
    public void UpdateLineLengths_VariousLengths(string s, int expected)
    {
        var m = CreateSut();
        m.UpdateLineLengths(5, s);
        Assert.True(m.TryGetLineLength(5, out var len));
        Assert.Equal(expected, len);
    }
}
