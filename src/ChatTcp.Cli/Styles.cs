namespace ChatTcp.Cli;

internal class Styles
{
    public const int MESSAGE_BLANK_LINES = 2;
    public const int PROMPT_JUMP_SPACING = 8;
    public const string PROMPT_PREFIX = "Chat>";

    public static string FormatChatMessage(ChatMessageDto chatMessage) => $"{chatMessage.Sender}: {chatMessage.Message}";
}
