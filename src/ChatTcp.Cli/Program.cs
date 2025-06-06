using System.Reactive.Linq;
using ChatTcp.Cli.Shell;
using ChatTcp.Cli.Shell.Models;
using ChatTcp.Cli.Shell.View;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var display = new Display(150, 100);
        var chatWindow = new ChatWindow(0, 0, 100, 20);
        var drawables = chatWindow.GetDrawables([
            ChatMessage.FromServer("Hello"),
            ChatMessage.FromOtherUser("Karl", "hello"),
            ChatMessage.FromCurrentUser("helljjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjnkjnkjnkjnkjnkjnkkjnjnjknnjo!"),
            ChatMessage.FromCurrentUser("helljjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjnkjnkjnkjnkjnkjnkkjnjnjknnjo!"),
            ChatMessage.FromOtherUser("Karl", "hello"),
            ChatMessage.FromCurrentUser("helljjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjjnkjnkjnkjnkjnkjnkkjnjnjknnjo!"),
            ]);
        display.Add(drawables);
        display.Render();

        Console.ReadKey();
        
        //using var cts = new CancellationTokenSource();

        //using var userInputManager = new ConsoleInput();
        //using var networkManager = new NetworkManager();
        //var éventStream = Observable.Merge(userInputManager.Events, networkManager.Events);

        //var appEventHandler = new AppEventHandler(cts);
        //éventStream.Subscribe(appEventHandler.Handle);

        //var renderer = new ConsoleOut();
        //appEventHandler.AppStateStream.Subscribe(renderer.Handle);
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
    }
}
