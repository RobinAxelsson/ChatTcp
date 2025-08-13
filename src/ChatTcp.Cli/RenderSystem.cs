
using System.Collections.Concurrent;
using System.Text;
using ChatTcp.Cli.Shell;

namespace ChatTcp.Cli;

public enum ApplicationState
{
    Network,
    Chat,
    Command
}

internal class Renderer
{
}

internal class RenderSystem
{
    private ConcurrentQueue<TextLayer> _toRenderQueue = new();
    private List<TextLayer> _textLayers;
    private readonly ConsoleAdapter _consoleAdapter;
    private readonly object lockObject = new object();
    private RenderSystemState _state;

    private List<(int row, string text)> LineBuffer = new(); //keep track of written lines
    private StringBuilder _sb = new StringBuilder();

    public RenderSystem(List<TextLayer> textLayers)
    : this(textLayers, ConsoleAdapter.Instance)
    {
    }

    // Constructor for unit testing
    internal RenderSystem(List<TextLayer> textLayers, ConsoleAdapter consoleAdapter)
    {
        _textLayers = textLayers;
        _consoleAdapter = consoleAdapter;
    }

    public void RequestClear()
    {
        _state = RenderSystemState.Clear;
    }

    public void RequestRender(TextLayer textLayer)
    {
        _toRenderQueue.Enqueue(textLayer);
    }

    public void Tick()
    {
        lock (lockObject)
        {
            if (_state == RenderSystemState.Clear)
            {
                _consoleAdapter.Clear();
                _sb.Clear();
                _toRenderQueue.Clear();

                for (int i = _textLayers.Count - 1; i >= 0; i--)
                {
                    if (_textLayers[i].State == TextLayerState.Rendered)
                    {
                        _textLayers[i].State = TextLayerState.Initialized;
                    }
                }

                _state = RenderSystemState.Render;
            }

            if (_state == RenderSystemState.Render && _toRenderQueue.TryPeek(out TextLayer? textLayerToRender))
            {
                var renderedLayerOrDefault = _textLayers.FirstOrDefault(x => x.State == TextLayerState.Rendered);

                bool alreadyRendered = renderedLayerOrDefault != null && textLayerToRender == renderedLayerOrDefault;

                if(!alreadyRendered)
                {
                    _consoleAdapter.Clear();
                    _sb.Clear();

                    RenderSystemHelpers.AppendRowsToStringBuilder(textLayerToRender.LayerBuffer, _sb);
                    WriteToConsoleOut(textLayerToRender, _sb, _consoleAdapter);
                }

                textLayerToRender.State = TextLayerState.Rendered;

                if (renderedLayerOrDefault != null)
                    renderedLayerOrDefault.State = TextLayerState.Initialized;

                if (!_toRenderQueue.TryDequeue(out var _))
                {
                    throw new InvalidStateException();
                }
            }
        }
    }

    private static void WriteToConsoleOut(TextLayer textLayerToRender, StringBuilder sb, ConsoleAdapter consoleAdapter)
    {
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.InputEncoding = Encoding.UTF8;
        Console.ForegroundColor = textLayerToRender.ForegroundColor;
        consoleAdapter.SetCursorPosition(0, 0);
        Console.Write(sb.ToString());
        consoleAdapter.SetCursorPosition(0, 0);
        Console.ResetColor();
    }

    private enum RenderCharOptions
    {
        NotSet,
        BothCharsExists,
        NoCharExist,
        OnlyRenderedExist,
        OnlyToRenderExist
    }
}

internal enum RenderSystemState
{
    Render,
    Clear,
}

internal class ConsoleAdapter
{
    private static ConsoleAdapter? _instance = null;
    private Action<int, int> _setCursorPosition = Console.SetCursorPosition;
    private Action _clear = Console.Clear;
    private ConsoleAdapter() { }

    internal static ConsoleAdapter Instance => _instance ??= new ConsoleAdapter();

    //test constructor
    internal ConsoleAdapter(Action<int, int> setCursorPosition, Action clear)
    {
        _setCursorPosition = setCursorPosition;
        _clear = clear;
    }

    public void SetCursorPosition(int x, int y) => _setCursorPosition?.Invoke(x, y);
    public void Clear() => _clear?.Invoke();
}
