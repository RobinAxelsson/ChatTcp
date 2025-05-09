namespace ChatTcp.Tests;

internal static class TaskCancellationTests
{
    private static async Task FailingProcess(CancellationToken ct)
    {
        Console.WriteLine("Failing process started...");

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(1000);
                throw new Exception("Simulating exception...");
            }
            finally
            {
                Console.WriteLine("Cleaning up failing process.");
            }
        }
    }

    private static async Task LongRunningProcess(CancellationToken ct)
    {
        Console.WriteLine("Long running process started...");
        try
        {
            while (true)
            {
                await Task.Delay(1000, ct); //throws an OperationCanceledException
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cancelling long running process...");
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static async Task CancellationListener(Action onCancellation, CancellationToken cancellationToken)
    {
        Console.WriteLine("CancellationListener started...");
        while (!cancellationToken.IsCancellationRequested)
        {
            if (Console.KeyAvailable)
            {
                onCancellation();
                Console.WriteLine("Cancellation listener end...");
                return;
            }
            await Task.Delay(1000);
        }
        Console.WriteLine("Cancelling cancellationlistener...");
    }

    public static async Task Run()
    {
        var source = new CancellationTokenSource();

        var failingProcess = FailingProcess(source.Token);
        var longRunningWithCancellation = LongRunningProcess(source.Token);
        var cancellationTokenListener = CancellationListener(() => {
            Console.WriteLine("Cancelling");
            source.Cancel();
        }, source.Token);

        var task = await Task.WhenAny(cancellationTokenListener, longRunningWithCancellation, failingProcess);
        Console.WriteLine("Canceling tasks...");
        source.Cancel();
        await Task.WhenAll(cancellationTokenListener, longRunningWithCancellation, failingProcess);

        Console.WriteLine("Exiting program");
    }
}

