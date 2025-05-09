namespace ChatTcp.Tests;
internal class Program
{
    private static async Task Main(string[] args)
    {
        await TaskCancellationTests.Run();
    }
}
