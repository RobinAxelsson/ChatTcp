using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ChatTcp.Cli.Shell;

internal sealed class ConsoleInputManager : IDisposable
{
    private readonly Subject<AppEvent> _eventStream = new();

    public IObservable<AppEvent> Events => _eventStream.AsObservable();

    public async Task Start(CancellationToken token)
    {
        int lastWidth = Console.WindowWidth;
        int lastHeight = Console.WindowHeight;

        _eventStream.OnNext(new ConsoleStartupEvent() { WindowHeight = lastHeight, WindowWidth = lastWidth });

        try
        {
            while (!token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    var appEvent = MapKeyToEvent(key);
                    if (appEvent != null)
                        _eventStream.OnNext(appEvent);
                }

                int width = Console.WindowWidth;
                int height = Console.WindowHeight;

                if (width != lastWidth || height != lastHeight)
                {
                    lastWidth = width;
                    lastHeight = height;
                    _eventStream.OnNext(new WindowResizedEvent(width, height));
                }

                await Task.Delay(ShellSettings.KeyHandlerDelay, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static AppEvent? MapKeyToEvent(ConsoleKeyInfo key)
    {
        if (key.Key == ConsoleKey.Backspace) return new BackspaceEvent();
        if (key.Key == ConsoleKey.Enter) return new PressEnterEvent();
        if (key.Key == ConsoleKey.Escape) return new PressEscapeEvent();
        if (key.Key == ConsoleKey.Z)
        {
            Console.CursorTop++;
        }
        if (key.Key == ConsoleKey.M)
        {
            Console.WindowTop++;
        }

        var modifiers = key.Modifiers;

        if ((modifiers & (ConsoleModifiers.Control | ConsoleModifiers.Alt)) == (ConsoleModifiers.Control | ConsoleModifiers.Alt))
        {
            if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || char.IsSymbol(key.KeyChar))
                return new CharInputEvent(key.KeyChar);
        }

        if (char.IsLetterOrDigit(key.KeyChar) || char.IsPunctuation(key.KeyChar) || char.IsSymbol(key.KeyChar) || key.KeyChar == ' ')
        {
            var c = key.KeyChar;
            if (modifiers == ConsoleModifiers.Shift && char.IsLower(c))
                c = char.ToUpper(c);

            return new CharInputEvent(c);
        }

        return null;
    }

    public void Dispose()
    {
        _eventStream.OnCompleted();
        _eventStream.Dispose();
    }
}
