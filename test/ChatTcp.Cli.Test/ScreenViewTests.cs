// ScreenViewTests.cs
using System;
using Xunit;

public class ScreenViewTests
{
    private static ScreenViewModel NewView() => new ScreenViewModel(ConsoleColor.Green, "T");

    [Fact]
    public void Initial_state_is_clean_with_defaults()
    {
        var v = new ScreenViewModel(ConsoleColor.Yellow, "Title");
        Assert.Equal("Title", v.Title);
        Assert.Equal(ConsoleColor.Yellow, v.ForegroundColor);
        Assert.Equal(0, v.FirstVisibleRow);
        Assert.Null(v.LastVisibleRow);
        Assert.False(v.IsDirty);
        Assert.Equal(0, v.Lines.Count);
    }

    [Fact]
    public void AppendLine_adds_line_and_returns_index()
    {
        var v = NewView();

        var idx0 = v.AppendLine("hello");
        Assert.Equal(0, idx0);
        Assert.Equal(1, v.Lines.Count);
        Assert.Equal("hello", v.Lines[idx0]);
        Assert.True(v.IsDirty);

        v.AcknowledgeRendered();
        var idx1 = v.AppendLine();
        Assert.Equal(1, idx1);
        Assert.Equal("", v.Lines[idx1]);
        Assert.True(v.IsDirty);
    }

    [Fact]
    public void AppendText_creates_intermediate_lines()
    {
        var v = NewView();
        v.AppendText("foo", 3);

        Assert.Equal(4, v.Lines.Count);
        Assert.Equal("", v.Lines[0]);
        Assert.Equal("", v.Lines[1]);
        Assert.Equal("", v.Lines[2]);
        Assert.Equal("foo", v.Lines[3]);
        Assert.True(v.IsDirty);
    }

    [Fact]
    public void AppendText_empty_or_null_is_noop()
    {
        var v = NewView();
        v.AppendText("", 0);
        v.AppendText(null, 0);

        Assert.False(v.IsDirty);
        Assert.Equal(0, v.Lines.Count);
    }

    [Fact]
    public void AppendChar_appends_character()
    {
        var v = NewView();
        v.AppendChar('A', 0);
        v.AppendChar('B', 0);

        Assert.Equal("AB", v.Lines[0]);
        Assert.True(v.IsDirty);
    }

    [Fact]
    public void SetLine_replaces_contents_and_can_create_line()
    {
        var v = NewView();
        v.AppendLine("old");
        v.AcknowledgeRendered();

        v.SetLine(0, "new");
        Assert.Equal("new", v.Lines[0]);
        Assert.True(v.IsDirty);

        v.AcknowledgeRendered();
        v.SetLine(2, null); // creates line 1 (empty) and line 2 (empty)
        Assert.Equal(3, v.Lines.Count);
        Assert.Equal("", v.Lines[1]);
        Assert.Equal("", v.Lines[2]);
        Assert.True(v.IsDirty);
    }

    [Fact]
    public void Clear_removes_all_lines_and_marks_dirty()
    {
        var v = NewView();
        v.AppendLine("a");
        v.AppendLine("b");
        v.AcknowledgeRendered();

        v.Clear();

        Assert.Equal(0, v.Lines.Count);
        Assert.True(v.IsDirty);
    }

    [Fact]
    public void Visible_window_updates_and_validates()
    {
        var v = NewView();
        v.AcknowledgeRendered();

        v.SetVisibleWindow(2, 5);
        Assert.Equal(2, v.FirstVisibleRow);
        Assert.Equal(5, v.LastVisibleRow);
        Assert.True(v.IsDirty);

        Assert.Throws<ArgumentOutOfRangeException>(() => v.SetVisibleWindow(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => v.SetVisibleWindow(0, -3));
    }

    [Fact]
    public void AcknowledgeRendered_clears_dirty_flag()
    {
        var v = NewView();
        v.AppendLine("x");
        Assert.True(v.IsDirty);

        v.AcknowledgeRendered();
        Assert.False(v.IsDirty);
    }

    [Fact]
    public void Title_and_foregroundColor_only_mark_dirty_on_change()
    {
        var v = NewView();
        v.AcknowledgeRendered();

        v.Title = "T"; // same value
        v.ForegroundColor = ConsoleColor.Green; // same value
        Assert.False(v.IsDirty);

        v.Title = "New";
        Assert.True(v.IsDirty);

        v.AcknowledgeRendered();
        v.ForegroundColor = ConsoleColor.Red;
        Assert.True(v.IsDirty);
    }

    [Fact]
    public void Lines_is_readonly_and_reflects_latest_content()
    {
        var v = NewView();
        var idx = v.AppendLine("a");

        var snapshot = v.Lines[idx];   // materialized string
        v.AppendText("b", idx);

        Assert.Equal("a", snapshot);       // old string unchanged
        Assert.Equal("ab", v.Lines[idx]);  // current view reflects updates

        Assert.False(v.Lines is System.Collections.Generic.List<string>);
    }

    [Fact]
    public void Negative_indices_throw()
    {
        var v = NewView();
        Assert.Throws<ArgumentOutOfRangeException>(() => v.AppendText("x", -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => v.AppendChar('x', -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => v.SetLine(-1, "x"));
    }
}
