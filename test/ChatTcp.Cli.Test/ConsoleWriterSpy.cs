using System.Text;

namespace ChatTcp.Cli.Shell;
public class ConsoleWriterSpy : TextWriter
{
    private readonly TextWriter _consoleOut;
    private readonly TextWriter _fileOut;

    private ConsoleWriterSpy()
    {
        var logDirectory = Path.Combine(Path.GetTempPath(), "chatTcp");
        var logFilePath = Path.Combine(logDirectory, "log.txt");

        Directory.CreateDirectory(logDirectory);

        _fileOut = new StreamWriter(logFilePath, append: true)
        {
            AutoFlush = true
        };

        _consoleOut = Console.Out;
        Console.SetOut(this);
    }

    public override Encoding Encoding => _consoleOut.Encoding;

    public override void Write(string? value)
    {
         _consoleOut.Write(value);
        _fileOut.Write(value);
    }

    public override void Write(char value)
    {
        _consoleOut.Write(value);
        _fileOut.Write(value);
    }

    public override void WriteLine(string? value)
    {
        _consoleOut.WriteLine(value);
        _fileOut.WriteLine(value);
    }

    public override void Flush()
    {
        _consoleOut.Flush();
        _fileOut.Flush();
    }
    private static ConsoleWriterSpy? _instance;
    internal static ConsoleWriterSpy Instance()
    {
        return _instance ?? (_instance = new ConsoleWriterSpy());
    }
}
