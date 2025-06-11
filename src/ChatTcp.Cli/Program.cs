using System.Reactive.Linq;
using ChatTcp.Cli.Shell.View;
namespace ChatTcp.Cli;

internal class ConsoleChat
{
    private List<Drawable> _prompt = new();
    private int _promptRow = 10;
    private int _nextChat = 0;
    private const string PROMPT_PREFIX = "Chat>";
    private const int PROMPT_JUMP = 5;

    public void Activate(CancellationToken token)
    {
        Console.SetCursorPosition(0, _promptRow);
        Console.Write(PROMPT_PREFIX);

        while (!token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Enter)
                {
                    if (!_prompt.Any() || _prompt.All(x => char.IsWhiteSpace(x.C)))
                    {
                        continue;
                    }

                    //erase prompt_prefix
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

                    //write the prompt in the message row
                    Console.SetCursorPosition(0, _nextChat);
                    Console.Write(string.Concat(_prompt.Select(x => x.C)));

                    int chatRows = _prompt.Select(x => x.Y).Distinct().Count();
                    _nextChat += chatRows;

                    //jump the prompt if messages reach
                    while(_nextChat >= _promptRow)
                    {
                        _promptRow += PROMPT_JUMP;
                    }

                    Console.SetCursorPosition(0, _promptRow);
                    Console.Write(PROMPT_PREFIX);

                    _prompt.Clear();
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

internal class Program
{
    private static async Task Main(string[] args)
    {
        var cts = new CancellationTokenSource();
        var consoleChat = new ConsoleChat();
        consoleChat.Activate(cts.Token);
        //using var cts = new CancellationTokenSource();
        //var token = cts.Token;
        //var prompt = new List<Drawable>();
        //int promptRow = 5;
        //int nextChat = 0;
        //const string start = "Chat>";

        //Console.SetCursorPosition(0, promptRow);
        //Console.Write(start);

        //while (!token.IsCancellationRequested)
        //{
        //    if (Console.KeyAvailable)
        //    {
        //        var key = Console.ReadKey(true);
        //        if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Enter)
        //        {
        //            if (!prompt.Any() || prompt.All(x => char.IsWhiteSpace(x.C)))
        //            {
        //                continue;
        //            }

        //            Console.SetCursorPosition(0, nextChat);
        //            Console.Write(string.Concat(prompt.Select(x => x.C)));
        //            //Console.Write("Send message:" + string.Concat(_prompt.Select(x => x.C)));

        //            nextChat++;

        //            if (nextChat >= promptRow)
        //            {
        //                promptRow = nextChat;
        //                Thread.Sleep(200);
        //                Console.SetCursorPosition(0, promptRow);
        //                Console.Write(start);
        //                prompt.Clear();
        //                continue;
        //            }

        //            Thread.Sleep(100);
        //            Console.SetCursorPosition(start.Length, promptRow);

        //            foreach (var d in prompt)
        //            {
        //                Console.SetCursorPosition(d.X - 1, d.Y);
        //                Console.Write(' ');
        //            }

        //            Console.SetCursorPosition(start.Length, promptRow);
        //            prompt.Clear();

        //            continue;
        //        }
        //        if (key.Key == ConsoleKey.Enter)
        //        {
        //            prompt.Add(new Drawable(Console.CursorLeft, Console.CursorTop, '\n'));
        //            Console.CursorLeft = 0;
        //            Console.CursorTop++;
        //            continue;
        //        }
        //        if (key.Key == ConsoleKey.Backspace)
        //        {
        //            if (!prompt.Any())
        //            {
        //                continue;
        //            }

        //            prompt.RemoveAt(prompt.Count - 1);

        //            if (Console.CursorLeft != 0)
        //            {
        //                Console.CursorLeft--;
        //                Console.Write(' ');
        //                Console.CursorLeft--;
        //            }
        //            else
        //            {
        //                Console.SetCursorPosition(prompt[^1].X, prompt[^1].Y);
        //            }

        //            continue;
        //        }
        //        Console.Write(key.KeyChar);
        //        prompt.Add(new Drawable(Console.CursorLeft, Console.CursorTop, key.KeyChar));
        //    }
        //}

        //using var userInputManager = new ConsoleInputManager();
        //using var networkManager = new NetworkManager();
        //var éventStream = Observable.Merge(userInputManager.Events, networkManager.Events);

        //var appEventHandler = new AppEventHandler(cts);
        //éventStream.Subscribe(appEventHandler.Handle);

        //var consoleOutputManager = new ConsoleOutputManager();
        ////appEventHandler.AppStateStream.Subscribe(consoleOutputManager.Handle);
        //appEventHandler.ChatMessageStream.Subscribe(networkManager.QueueChatMessage);

        //var tasks = new[]
        //{
        //    userInputManager.Start(cts.Token),
        //    networkManager.StartAsync(cts.Token)
        //};

        //var firstTask = Task.WhenAny(tasks);

        //if (firstTask.IsFaulted)
        //{
        //    cts.Cancel();
        //}

        //await Task.WhenAll(tasks);


        //ConsoleInput
        //NetworkClient

        //ChatWindow
        //InputArea
        //NetworkClient

        //User quit
        //Send message
        //Input key
        //Char
        //commands keys

        //renders the chats
        //readonly chat area (Scrollable)
        //dont need to have memory
        //AddCurrentUserMessage()
        //AddChatMessage()
        //just prints to the screen next step

        //when overflowing should scroll and rerender the input box

        //Sliding input area
        //listening for key input
        //Task Start()
        //Move(int rows)
        //clearable
        //own memory
    }
}
