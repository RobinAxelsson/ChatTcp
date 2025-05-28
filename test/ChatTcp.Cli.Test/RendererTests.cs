using System.Linq.Expressions;
using System.Text;
using ChatTcp.Cli.ConsoleUi;
using Xunit;

namespace ChatTcp.Cli.Test;
public class RendererTests
{
    [Fact]
    public void FrameSize()
    {
        char[,] frame = new char[10, 3];
        Assert.Equal(10, frame.GetLength(0));
        Assert.Equal(3, frame.GetLength(1));
    }

    [Fact]
    public void FillChatFrame_CurrentAndResponse_CurrentUserMessagesToTheRight()
    {
        int width = 10;
        int height = 3;
        var frame = new char[width, height];

        var message0 = ChatMessage.FromOtherUser("Ben", "Hey");
        var message1 = ChatMessage.FromCurrentUser("Hey");

        var messages = new List<ChatMessage>() { message0, message1 };

        Renderer.FillChatFrame(width, height, frame, messages);

        var result = ConvertFrame(frame);

        Assert.Equal("Ben: Hey__", result[0]);
        Assert.Equal("__________", result[1]);
        Assert.Equal("_______Hey", result[2]);
    }

    [Fact]
    public void FillFrame_SingleElement_MessageDefaults()
    {
        int width = 10;
        int height = 4;
        var frame = new char[width, height];

        var element = new Element()
        {
            InnerText = "Hello world"
        };
    }

    [Fact]
    public void FillChatFrame_Overflow_TheFirstShouldBeTrimmed()
    {
        int width = 10;
        int height = 4;
        var frame = new char[width, height];

        var message0 = ChatMessage.FromOtherUser("Ben", "Hey");
        var message1 = ChatMessage.FromCurrentUser("Hey");
        var message2 = ChatMessage.FromOtherUser("Tom", "Hey");

        var messages = new List<ChatMessage> { message0, message1, message2 };

        Renderer.FillChatFrame(width, height, frame, messages);

        var result = ConvertFrame(frame);

        Assert.Equal("_______Hey", result[0]);
        Assert.Equal("__________", result[1]);
        Assert.Equal("Tom: Hey__", result[2]);
        Assert.Equal("__________", result[3]);
    }

    [Fact]
    public void FillChatFrame_SameSender_NoSpacing()
    {
        int width = 10;
        int height = 3;
        var frame = new char[width, height];

        var message0 = ChatMessage.FromCurrentUser("Hey");
        var message1 = ChatMessage.FromCurrentUser("You");

        var messages = new List<ChatMessage> { message0, message1 };

        Renderer.FillChatFrame(width, height, frame, messages);

        var result = ConvertFrame(frame);

        Assert.Equal("_______Hey", result[0]);
        Assert.Equal("_______You", result[1]);
        Assert.Equal("__________", result[2]);
    }

    [Fact]
    public void FillChatFrame_TextOverflow_Newlines()
    {
        int width = 4;
        int height = 2;
        var frame = new char[width, height];

        var message0 = ChatMessage.FromCurrentUser("Come on");

        var messages = new List<ChatMessage> { message0 };

        Renderer.FillChatFrame(width, height, frame, messages);

        var result = ConvertFrame(frame);

        Assert.Equal("Come", result[0]);
        Assert.Equal("_on_", result[1]);
    }

    private static string[] ConvertFrame(char[,] frame)
    {
        var xMax = frame.GetLength(0);
        var yMax = frame.GetLength(1);
        var sb = new StringBuilder(frame.Length + yMax); //extra row for newline
        var rows = new string[yMax];

        for (int y = 0; y < yMax; y++)
        {
            for (int x = 0; x < xMax; x++)
            {
                char c = frame[x, y];
                if (c == '\0')
                {
                    c = '_';
                }

                sb.Append(c);
            }
            rows[y] = sb.ToString();
            sb.Clear();
        }

        return rows;
    }
}
