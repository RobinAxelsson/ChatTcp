using ChatTcp.Cli.ConsoleUi;

namespace ChatTcp.Cli;

internal class Chat
{
    private record Message(string user, string message);

    private readonly Action<CharElement> _addCharElement;
    private readonly AppWindow _appWindow;
    private List<Message> _messages = new();

    public Chat(Action<CharElement> addCharElement, AppWindow appWindow)
    {
        _addCharElement = addCharElement;
        _appWindow = appWindow;
    }

    public void SubmitHostMessage(string message)
    {
        _messages.Add(new Message("Me", message));

        var textLocation = _appWindow.GetTextLoc();
        var startX = textLocation.end.X - message.Length;
        var startY = textLocation.start.Y + _messages.Count;

        int messagePtr = 0;
        for (int x = startX; x < message.Length + startX; x++, messagePtr++)
        {
            var element = new CharElement() { Char = message[messagePtr], X = x, Y = startY };
            _addCharElement(element);
        }
    }

    public void AddPeerMessage(string user, string message)
    {
        _messages.Add(new Message(user, message));
    }
}
