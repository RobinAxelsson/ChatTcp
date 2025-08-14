//Instructions chatGpt:
//You are a .NET senior developer
//You value simple design solutions over complex
//You use braces after if statements if they are oneliners
//Use => for oneline Get Properties else braces
//skip braces for throw new Exception if possible
//Few comments if not other specified
//You have memory allocation in mind but dont sacrifice simplicity
//You like to keep entities easy to access and easy to iterate
//You value entity data integrity
//You throw InvalidStateException on any bad state
//don't guard for null when it cant be null
//When you are ready question if anything is wrong, or not compiling
//dont reaply previous code if not instructed
//Always fix TODOs and remove the TODO comment if implemented
//Never remove these instruction lines

namespace ChatTcp.Cli;

internal abstract class TaskContext
{
}

internal sealed class AcceptClientTaskContext : TaskContext
{
    internal Listener Listener { get; }
    internal AcceptClientTaskContext(Listener listener)
    {
        Listener = listener;
    }
}

internal sealed class SendPacketTaskContext : TaskContext
{
    internal WirePacketDto Packet { get; }
    internal SendPacketTaskContext(WirePacketDto packet)
    {
        Packet = packet;
    }

    public WirePacketDto Result => Packet;
}

internal sealed class MessageReceivedTaskContext : TaskContext
{
    internal WirePacketDto Packet { get; }
    internal MessageReceivedTaskContext(WirePacketDto packet)
    {
        Packet = packet;
    }

    public WirePacketDto Result => Packet;
}
