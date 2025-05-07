using System.Reactive.Subjects;

namespace ChatTcp.Cli.ConsoleUi;

internal class KeyHandler : IDisposable
{
    private CancellationTokenSource _cts = new();
    private readonly Subject<ConsoleKeyInfo> _keyStream = new();

    public IObservable<ConsoleKeyInfo> KeyStream => _keyStream;
    public async Task Start()
    {
        try
        {
            while (!_cts.Token.IsCancellationRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    _keyStream.OnNext(key);

                    if (key.Key == ConsoleKey.Escape)
                    {
                        Stop();
                        break;
                    }
                }
                else
                {
                    // Use a small async delay for efficiency
                    await Task.Delay(10, _cts.Token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation, no need to propagate
        }
        catch (Exception ex)
        {
            _keyStream.OnError(ex);
        }
        finally
        {
            _keyStream.OnCompleted();
        }
    }

    public void Stop()
    {
        if (!_cts.IsCancellationRequested)
        {
            _cts.Cancel();
        }
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
        _keyStream.Dispose();
    }
}
