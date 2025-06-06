using ChatTcp.Cli.Shell.Models;

namespace ChatTcp.Cli.Shell.View;

internal class ChatWindow
{
    public ChatWindow(int x1, int y1, int x2, int y2)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;

        if (x1 > x2)
            throw new ShellException($"x1: {x1} can not be bigger then x2: {x2}");

        if (y1 > y2)
            throw new ShellException($"y1: {y1} can not be bigger then y2: {y2}");
    }

    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }

    public Drawable[] GetDrawables(IEnumerable<ChatMessage> chatMessages)
    {
        if (chatMessages == null || !chatMessages.Any())
            return [];

        int y = Y1;
        var textElements = new List<ChatBubble>();
        int chatWidth = X2 - X1;
        int elementWidth = chatWidth - chatWidth / 4;
        bool isGroupChat = chatMessages.DistinctBy(x => x.SenderName).Count() > 2;

        ChatMessage? lastMessage = null;
        foreach (var chatMessage in chatMessages)
        {
            bool sameSenderAsLastMessage = lastMessage != null && chatMessage.SenderName == lastMessage.SenderName;

            if (lastMessage != null && !sameSenderAsLastMessage)
                y++;
            
            bool useNamePrefix = true;

            if(chatMessage.SenderType == SenderType.CurrentUser || sameSenderAsLastMessage || !isGroupChat)
            {
                useNamePrefix = false;
            }

            string text = $"{(useNamePrefix ? $"{chatMessage.SenderName}: ": "")}{chatMessage.Content}";

            var textElement = new ChatBubble(text, elementWidth);

            textElement.Y = y;
            textElement.X = X1;

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
        if(yMax > Y2)
        {
            int diff = yMax - Y2;
            
            textElements.ForEach(e => e.Y -= diff);
        }

        var posetiveElements = textElements.Where(x => x.Y > -1).ToList();

        var drawables = textElements.SelectMany(x => x.GetDrawables()).Where(d => d.Y > -1).ToArray();

        return drawables;
    }
}
