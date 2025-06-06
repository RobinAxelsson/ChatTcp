using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell.View;

internal class ChatWindow
{
    public ChatWindow(int x0, int y0, int x1, int y1)
    {
        if (x0 > x1)
            throw new ShellException($"x0: {x0} can not be bigger than x1: {x1}");

        if (y0 > y1)
            throw new ShellException($"y0: {y0} can not be bigger than y1: {y1}");

        X0 = x0;
        Y0 = y0;
        X1 = x1;
        Y1 = y1;
    }


    public int X0 { get; set; }
    public int Y0 { get; set; }
    public int X1 { get; set; }
    public int Y1 { get; set; }

    public Drawable[] GetDrawables(IEnumerable<ChatMessage> chatMessages)
    {
        if (chatMessages == null || !chatMessages.Any())
            return [];

        int y = Y0;
        var textElements = new List<TextElement>();
        int chatWidth = X1 - X0;
        int elementWidth = chatWidth - chatWidth / 4;
        bool isGroupChat = chatMessages.DistinctBy(x => x.SenderName).Count() > 2;

        ChatMessage? lastMessage = null;
        foreach (var chatMessage in chatMessages)
        {
            bool sameSenderAsLastMessage = lastMessage != null && chatMessage.SenderName == lastMessage.SenderName;

            //Add linespacing between messages on the left
            if (chatMessage.SenderType != SenderType.CurrentUser && lastMessage != null && !sameSenderAsLastMessage)
                y++;
            
            bool useNamePrefix = true;

            if(chatMessage.SenderType == SenderType.CurrentUser || sameSenderAsLastMessage || !isGroupChat)
            {
                useNamePrefix = false;
            }

            string text = $"{(useNamePrefix ? $"{chatMessage.SenderName}: ": "")}{chatMessage.Content}";

            var textElement = new TextElement(text, elementWidth);

            textElement.Y = y;
            textElement.X = X0;

            if(chatMessage.SenderType == SenderType.CurrentUser)
            {
                var width = textElement.Width;
                textElement.X = chatWidth - width;
            }

            y += textElement.Height;

            textElements.Add(textElement);

            lastMessage = chatMessage;
        }

        var yMax = textElements.Select(e => e.Y).Max();

        //chat overflow move to the latest chats
        if(yMax > Y1)
        {
            int diff = yMax - Y1;
            
            textElements.ForEach(e => e.Y -= diff);
        }

        var posetiveElements = textElements.Where(x => x.Y > -1).ToList();

        var drawables = textElements.SelectMany(x => x.GetDrawables()).Where(d => d.Y > -1).ToArray();

        return drawables;
    }
}
