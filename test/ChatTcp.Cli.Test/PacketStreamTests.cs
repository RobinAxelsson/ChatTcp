using System.Text;
using ChatTcp.Kernel;
using Xunit;

namespace ChatTcp.Cli.Shell;

public class PacketStreamTests
{
    [Fact]
    public async void WritePacket_And_ReadPacket_ShouldSerializeAndDeserializeCorrectly()
    {
        var message = new ChatMessageDto("Alice", "Hello");

        using var ms = new MemoryStream();
        await PacketStream.WritePacketAsync(message, ms, CancellationToken.None);
        ms.Seek(0, SeekOrigin.Begin);

        var result = await PacketStream.ReadPacketAsync(ms, CancellationToken.None) as ChatMessageDto;

        Assert.NotNull(result);
        Assert.Equal("Hello", result.Message);
        Assert.Equal("Alice", result.Sender);
    }

    [Fact]
    public async void ReadPacket_ShouldThrow_WhenVersionIsInvalid()
    {
        using var ms = new MemoryStream();
        ms.WriteByte(0); // invalid version
        ms.WriteByte((byte)PayloadType.ChatMessage);
        ms.WriteByte(1);
        ms.Write(Encoding.UTF8.GetBytes("{}"), 0, 2);
        ms.Seek(0, SeekOrigin.Begin);

        var ex = await Assert.ThrowsAsync<ChatTcpKernelException>(() => PacketStream.ReadPacketAsync(ms, CancellationToken.None));
        Assert.Contains("Only v1 is allowed", ex.Message);
    }

    [Fact]
    public async void ReadPacket_ShouldThrow_WhenPayloadTypeIsInvalid()
    {
        using var ms = new MemoryStream();
        ms.WriteByte(1); // valid version
        ms.WriteByte((byte)PayloadType.Unknown); // unsupported payload type
        ms.WriteByte(1);
        ms.Write(Encoding.UTF8.GetBytes("{}"), 0, 2);
        ms.Seek(0, SeekOrigin.Begin);

        var ex = await Assert.ThrowsAsync<ChatTcpKernelException>(() => PacketStream.ReadPacketAsync(ms, CancellationToken.None));
        Assert.Contains("only chatmessage type is allowed", ex.Message);
    }

    [Fact]
    public async void WritePacket_ShouldThrow_WhenPayloadTooLong()
    {
        var longMessage = new ChatMessageDto("Bob", new string('a', 260));

        using var ms = new MemoryStream();

        var ex = await Assert.ThrowsAsync<ChatTcpKernelException>(() => PacketStream.WritePacketAsync(longMessage, ms, CancellationToken.None));
        Assert.Contains("Payload is to long", ex.Message);
    }

    [Fact]
    public async void ReadPacket_ShouldThrow_WhenPayloadMalformed()
    {
        using var ms = new MemoryStream();
        ms.WriteByte(1); // version
        ms.WriteByte((byte)PayloadType.ChatMessage);
        ms.WriteByte(5);
        ms.Write(Encoding.UTF8.GetBytes("abcde")); // invalid JSON
        ms.Seek(0, SeekOrigin.Begin);

        var ex = await Assert.ThrowsAsync<ChatTcpKernelException>(() => PacketStream.ReadPacketAsync(ms, CancellationToken.None));
        Assert.Contains("could not be deserialized", ex.Message);
    }
}
