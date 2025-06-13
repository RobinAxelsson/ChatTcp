using System.Collections.Concurrent;
using System.Reactive.Linq;
using ChatTcp.Cli.Shell.Models;
using ChatTcp.Cli.Shell.View;
namespace ChatTcp.Cli;

internal class ConsoleChat
{
    private List<Drawable> _prompt = new();
    private int _promptRow = 3;
    private int _nextChatRow = 0;
    private const string PROMPT_PREFIX = "Chat>";
    private const int PROMPT_JUMP = 5;
    private ConcurrentQueue<string> _messageQueue = new();
    public Action<ChatMessage>? OnCurrentUserMessageSubmitted { private get; set; }

    public async Task DebugQueue(CancellationToken token)
    {
        while(!token.IsCancellationRequested)
        {
            _messageQueue.Enqueue("Server: hello");
            await Task.Delay(2000);
        }
    }

    public void AddMessage(string message)
    {
        _messageQueue.Enqueue(message);
    }

    public void Activate(CancellationToken token)
    {
        Console.SetCursorPosition(0, _promptRow);
        Console.Write(PROMPT_PREFIX);

        while (!token.IsCancellationRequested)
        {
            if (_messageQueue.TryDequeue(out var message))
            {
                int endChatRow = _nextChatRow + message.Where(x => x == '\n').Count() + 1;

                int newPromptRow = _promptRow;

                //jump the prompt if messages reach
                while (endChatRow >= newPromptRow)
                {
                    newPromptRow += PROMPT_JUMP;
                }

                int promptDiff = newPromptRow - _promptRow;

                var cursorPosition = Console.GetCursorPosition();
                //Console.CursorVisible = false;

                bool movePrompt = newPromptRow != _promptRow;

                if (movePrompt)
                {
                    //erase prompt_prefix, it is important to erase first before writing the message
                    Console.SetCursorPosition(0, _promptRow);
                    for (int i = 0; i < PROMPT_PREFIX.Length; i++)
                    {
                        Console.Write(" ");
                    }

                    //erase prompt
                    foreach (var d in _prompt)
                    {
                        int x = d.X == 0 ? 0 : d.X - 1;

                        Console.SetCursorPosition(x, d.Y);
                        Console.Write(' ');
                    }

                    //restore prompt
                    Console.SetCursorPosition(0, newPromptRow);
                    Console.Write(PROMPT_PREFIX);

                    _prompt.ForEach(d =>
                    {
                        int x = d.X == 0 ? 0 : d.X - 1;
                        Console.SetCursorPosition(d.X - 1, d.Y + promptDiff);
                        Console.Write(d.C);
                    });

                    _promptRow = newPromptRow;
                }

                //write new message
                Console.SetCursorPosition(0, _nextChatRow);
                Console.Write(message);
                _nextChatRow = endChatRow;

                Console.SetCursorPosition(cursorPosition.Left, cursorPosition.Top + promptDiff);
                Console.CursorVisible = true;
                Thread.Sleep(200);
            }
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Enter)
                {
                    if (!_prompt.Any() || _prompt.All(x => char.IsWhiteSpace(x.C)))
                    {
                        continue;
                    }

                    foreach (var d in _prompt)
                    {
                        int x = d.X == 0 ? 0 : d.X - 1;

                        Console.SetCursorPosition(x, d.Y);
                        Console.Write(' ');
                    }

                    var chatEnd = _prompt.Select(x => x.Y).Distinct().Count() + _nextChatRow;
                    bool jumpPrompt = chatEnd >= _promptRow;

                    if (jumpPrompt)
                    {
                        //erase prompt_prefix
                        Console.SetCursorPosition(0, _promptRow);
                        for (int i = 0; i < PROMPT_PREFIX.Length; i++)
                        {
                            Console.Write(" ");
                        }

                        while (chatEnd >= _promptRow)
                        {
                            _promptRow += PROMPT_JUMP;
                        }

                        Console.SetCursorPosition(0, _promptRow);
                        Console.Write(PROMPT_PREFIX);
                    }

                    //write message
                    var prompt = string.Concat(_prompt.Select(x => x.C));
                    Console.SetCursorPosition(0, _nextChatRow);
                    Console.Write(prompt);

                    OnCurrentUserMessageSubmitted?.Invoke(ChatMessage.FromCurrentUser(prompt));

                    Console.SetCursorPosition(PROMPT_PREFIX.Length, _promptRow);

                    _nextChatRow = chatEnd;
                    _prompt.Clear();
                    Thread.Sleep(500);
                    continue;
                }
                if (key.Key == ConsoleKey.Enter)
                {
                    _prompt.Add(new Drawable(Console.CursorLeft, Console.CursorTop, '\n'));
                    Console.CursorLeft = 0;
                    Console.CursorTop++;
                    continue;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (!_prompt.Any())
                    {
                        continue;
                    }

                    _prompt.RemoveAt(_prompt.Count - 1);

                    bool isFirstPromptRowStart = Console.CursorTop == _promptRow && Console.CursorLeft == PROMPT_PREFIX.Length;
                    bool isOtherRowStart = Console.CursorTop > _promptRow && Console.CursorLeft == 0;

                    if (!isOtherRowStart && !isFirstPromptRowStart)
                    {
                        Console.CursorLeft--;
                        Console.Write(' ');
                        Console.CursorLeft--;
                    }

                    if(isOtherRowStart)
                    {
                        Console.SetCursorPosition(_prompt[^1].X, _prompt[^1].Y);
                    }

                    continue;
                }

                Console.Write(key.KeyChar);
                _prompt.Add(new Drawable(Console.CursorLeft, Console.CursorTop, key.KeyChar));
            }
        }
    }
}
