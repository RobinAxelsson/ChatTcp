using System.Reactive.Subjects;

namespace ChatTcp.Cli.ConsoleUi;

internal class KeyHandler : IDisposable
{
    private readonly Subject<ConsoleKeyInfo> _keyStream = new();

    public IObservable<ConsoleKeyInfo> KeyStream => _keyStream;
    public async Task Start(CancellationToken cancellationToken)
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

                await Task.Delay(ShellSettings.KeyHandlerDelay, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelling keyhandler...");
            return;
        }
    }

    public void Dispose()
    {
        _keyStream.Dispose();
    }
}
