//using ChatTcp.Kernel;
//using ChatTcp.Server;
//using Xunit;

//namespace ChatTcp.Cli.Shell;

//public class ClientServerTests
//{
//    [Fact]
//    public async void AliasRequest_SendValid_ShouldGetResponse()
//    {
//        var cts = new CancellationTokenSource();
//        var server = new ChatServer();

//        var client = new NetworkManager();

//        //Global scope? Who owns the data? All access to it?
//        //TcpClients
//        //Users
//        //Messages
//        //Requests

//        var serverTask = server.Run(cts.Token);
//        var clientTask = client.StartAsync(cts);


//        await Task.Delay(1000);

//        var joinChatResponseTask = client.SendJoinChatRequest(new JoinChatDto("Bob"), cts.Token);

//        var firstTask = await Task.WhenAny(serverTask, clientTask, joinChatResponseTask);
//        try
//        {
//            if (firstTask.IsFaulted)
//            {
//                await firstTask;
//            }

//            var joinChatResponse = await joinChatResponseTask;
//            Assert.NotNull(joinChatResponse);
//        }
//        finally
//        {
//            cts.Cancel();
//            await Task.WhenAll(serverTask, clientTask, joinChatResponseTask);
//        }
//    }
//}
