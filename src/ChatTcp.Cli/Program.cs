using System.Dynamic;
using System.Reactive.Linq;
using ChatTcp.Cli.Shell;
using ChatTcp.Cli.Shell.View;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var consoleIn = new List<Drawable>();
        int promptRow = 5;
        int nextChat = 0;
        const string start = "Chat>";

        Console.SetCursorPosition(0, promptRow);
        Console.Write(start);

        while (!token.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Modifiers == ConsoleModifiers.Control && key.Key == ConsoleKey.Enter)
                {
                    if (!consoleIn.Any() || consoleIn.All(x => char.IsWhiteSpace(x.C)))
                    {
                        continue;
                    }

                    Console.SetCursorPosition(0, nextChat);
                    Console.Write(string.Concat(consoleIn.Select(x => x.C)));
                    //Console.Write("Send message:" + string.Concat(consoleIn.Select(x => x.C)));

                    nextChat++;

                    if(nextChat >= promptRow)
                    {
                        promptRow = nextChat;
                        Thread.Sleep(200);
                        Console.SetCursorPosition(0, promptRow);
                        Console.Write(start);
                        consoleIn.Clear();
                        continue;
                    }

                    Thread.Sleep(100);
                    Console.SetCursorPosition(start.Length, promptRow);

                    foreach (var d in consoleIn)
                    {
                        Console.SetCursorPosition(d.X - 1, d.Y);
                        Console.Write(' ');
                    }

                    Console.SetCursorPosition(start.Length, promptRow);
                    consoleIn.Clear();

                    continue;
                }
                if (key.Key == ConsoleKey.Enter)
                {
                    consoleIn.Add(new Drawable(Console.CursorLeft, Console.CursorTop, '\n'));
                    Console.CursorLeft = 0;
                    Console.CursorTop++;
                    continue;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (!consoleIn.Any())
                    {
                        continue;
                    }

                    consoleIn.RemoveAt(consoleIn.Count - 1);

                    if (Console.CursorLeft != 0)
                    {
                        Console.CursorLeft--;
                        Console.Write(' ');
                        Console.CursorLeft--;
                    }
                    else
                    {
                        Console.SetCursorPosition(consoleIn[^1].X, consoleIn[^1].Y);
                    }

                    continue;
                }
                Console.Write(key.KeyChar);
                consoleIn.Add(new Drawable(Console.CursorLeft, Console.CursorTop, key.KeyChar));
            }
        }

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
