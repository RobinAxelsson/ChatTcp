using System.Reactive.Subjects;

namespace ChatTcp.Cli.ConsoleUi;

internal class KeyHandler : IDisposable
{
    private readonly Subject<ConsoleKeyInfo> _keyStream = new();

    public IObservable<ConsoleKeyInfo> KeyStream => _keyStream;
    public async Task Start(AppLifecycle appLifecycle)
    {
        try
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    _keyStream.OnNext(key);
                }
                else
                {
                    await Task.Delay(ShellSettings.KeyHandlerDelay, appLifecycle.Token).ConfigureAwait(false);
                }

                if (appLifecycle.Token.IsCancellationRequested)
                {
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("KeyHandler threw an exception");
            Console.WriteLine(ex);
            appLifecycle.RequestShutdown();
        }
        finally
        {
            _keyStream.OnCompleted();
        }
    }

    public void Dispose()
    {
        _keyStream.Dispose();
    }
}
