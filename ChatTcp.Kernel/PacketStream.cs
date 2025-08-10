using System.Text;
using System.Text.Json;

namespace ChatTcp.Kernel;

public class PacketStream
{
    private static Dictionary<PayloadType, Type> _typeDict;
    private static Dictionary<Type, PayloadType> _typeDictReverse;

    static PacketStream()
    {
        _typeDict = new() { { PayloadType.ChatMessage, typeof(ChatMessageDto) } };
        _typeDictReverse = new();

        foreach (var kvp in _typeDict)
        {
            _typeDictReverse[kvp.Value] = kvp.Key;
        }
    }
    
    public static async Task<WirePacketDto> ReadPacketAsync(Stream stream, CancellationToken ct)
    {
        var buffer = new byte[3];

        var read = await stream.ReadAsync(buffer, 0, 3, ct);
        var version = buffer[0];

        if (version != 1)
            throw new ChatTcpKernelException("Only v1 is allowed this is: " + version);

        var payloadType = (PayloadType)buffer[1];

        if(payloadType != PayloadType.ChatMessage)
            throw new ChatTcpKernelException("only chatmessage type is allowed this was: " + (int)payloadType);

        var payloadLength = buffer[2];

        if (payloadLength < 1 || payloadLength > 255)
            throw new ChatTcpKernelException("Payload length exceeds size of byte, payload length: " + payloadLength);

        buffer = new byte[payloadLength];
        await stream.ReadAsync(buffer, 0, payloadLength, ct);

        var sPayload = Encoding.UTF8.GetString(buffer);

        WirePacketDto? packetDto;
        try
        {
            packetDto = JsonSerializer.Deserialize(sPayload, _typeDict[payloadType]) as WirePacketDto;
        }
        catch (JsonException ex)
        {
            throw new ChatTcpKernelException($"Json packet payload could not be deserialized. PayloadType: '{payloadType}' payload: {sPayload}", ex);
        }

        if (packetDto == null)
            throw new ChatTcpKernelException("The payload: " + sPayload + " could not be deserialized to type: " + _typeDict[payloadType].Name);

        return packetDto;
    }

    public static async Task WritePacketAsync

        <T>(T packetDto, Stream stream, CancellationToken ct) where T : WirePacketDto
    {
        const byte version = 1;

        byte payloadType = (byte)_typeDictReverse[typeof(T)];

        if (payloadType != (byte)PayloadType.ChatMessage)
            throw new ChatTcpKernelException("only chatmessage type is allowed this was: " + (int)payloadType);

        var payload = JsonSerializer.Serialize(packetDto);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        if (payloadBytes.Length > 255)
            throw new ChatTcpKernelException("Payload is to long, allowed bytes are 255 this was: " + payloadBytes.Length);

        byte bytesLength = (byte)payloadBytes.Length;

        var buffer = new byte[bytesLength + 3];
        buffer[0] = version;
        buffer[1] = payloadType;
        buffer[2] = bytesLength;

        for ( int i = 0; i < payloadBytes.Length; i++)
        {
            buffer[i+3] = payloadBytes[i];
        }

        await stream.WriteAsync(buffer, ct);
    }
}

internal enum PayloadType
{
    Unknown = 0,
    ChatMessage = 1
}
