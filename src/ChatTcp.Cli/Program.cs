using System.Drawing;
using System.Reactive.Linq;
using ChatTcp.Cli.Shell;
using ChatTcp.Cli.Shell.Models;
using ChatTcp.Cli.Shell.View;
namespace ChatTcp.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var consoleOut = new ConsoleOutput();

        var appState = new AppState()
        {
            Messages = [
            ChatMessage.FromServer("You are connected"),
            ChatMessage.FromServer("Have fun"),
            ChatMessage.FromOtherUser("Karl", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus interdum ex sit amet augue maximus, a rhoncus magna cursus. Pellentesque et turpis sit amet quam sagittis accumsan. In id sem ornare, ornare ligula et, iaculis eros. Aenean dignissim elit non magna lobortis, at iaculis ex lacinia. Sed a diam nec nisl mollis dignissim sed ut purus. Nunc sit amet ipsum suscipit, consectetur mauris vel, posuere augue. Sed at est non nulla tincidunt vehicula. Nullam molestie gravida arcu. Vivamus pellentesque neque at purus consequat, sed commodo magna maximus."),
            ChatMessage.FromCurrentUser("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus interdum ex sit amet augue maximus, a rhoncus magna cursus. Pellentesque et turpis sit amet quam sagittis accumsan. In id sem ornare, ornare ligula et, iaculis eros. Aenean dignissim elit non magna lobortis, at iaculis ex lacinia. Sed a diam nec nisl mollis dignissim sed ut purus. Nunc sit amet ipsum suscipit, consectetur mauris vel, posuere augue. Sed at est non nulla tincidunt vehicula. Nullam molestie gravida arcu. Vivamus pellentesque neque at purus consequat, sed commodo magna maximus."),
            ChatMessage.FromOtherUser("Liza", "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus interdum ex sit amet augue maximus, a rhoncus magna cursus. Pellentesque et turpis sit amet quam sagittis accumsan. In id sem ornare, ornare ligula et, iaculis eros. Aenean dignissim elit non magna lobortis, at iaculis ex lacinia. Sed a diam nec nisl mollis dignissim sed ut purus. Nunc sit amet ipsum suscipit, consectetur mauris vel, posuere augue. Sed at est non nulla tincidunt vehicula. Nullam molestie gravida arcu. Vivamus pellentesque neque at purus consequat, sed commodo magna maximus."),
            ],
            CursorIndex = -1,
            InputBuffer = "Hello world",
            WindowHeight = Console.WindowHeight,
            WindowWidth = Console.WindowWidth,
        };

        consoleOut.Handle(appState);

        while (true)
        {
            Console.Read();
        }
        
        //using var cts = new CancellationTokenSource();

        //using var userInputManager = new ConsoleInput();
        //using var networkManager = new NetworkManager();
        //var éventStream = Observable.Merge(userInputManager.Events, networkManager.Events);

        //var appEventHandler = new AppEventTranslator(cts);
        //éventStream.Subscribe(appEventHandler.Handle);

        //var renderer = new ConsoleOutput();
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
