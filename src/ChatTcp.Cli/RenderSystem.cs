
using System.Collections.Concurrent;
using System.Text;
using ChatTcp.Cli.Shell;

namespace ChatTcp.Cli;

internal class RenderSystem
{
    private ConcurrentQueue<TextLayer> _toRenderQueue = new();
    private List<TextLayer> _textLayers;
    private readonly ConsoleAdapter _consoleAdapter;
    private Dictionary<(int X, int Y), List<LayerChar>> _positionCharDict = new();
    private StringBuilder _sb = new StringBuilder();
    private readonly object lockObject = new object();
    private RenderSystemState _state;

    public RenderSystem(): this(new List<TextLayer>(), new ConsoleAdapter())
    {
    }

    public RenderSystem(List<TextLayer> textLayers)
    : this(textLayers, new ConsoleAdapter())
    {
    }

    // For tests
    internal RenderSystem(List<TextLayer> textLayers, ConsoleAdapter consoleAdapter)
    {
        _textLayers = textLayers;
        _consoleAdapter = consoleAdapter;
        MapTextLayerCharsToPositionDictionary(_positionCharDict, _textLayers);
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

            if (_toRenderQueue.TryPeek(out TextLayer? textLayerToRender))
            {

                _sb.Clear();

                AppendLayerToRenderToStringBuilder(_positionCharDict, textLayerToRender, _sb);

                RenderToConsole(_sb, textLayerToRender.ForegroundColor);

                UpdateLayerStates(textLayerToRender, _textLayers, _toRenderQueue);
            }
        }
    }

    //internal for testing
    internal static void MapTextLayerCharsToPositionDictionary(Dictionary<(int X, int Y), List<LayerChar>> positionCharDict, List<TextLayer> textLayers)
    {
        foreach (var textLayer in textLayers)
        {
            int y = 0, x = 0;

            foreach (var c in textLayer.Text)
            {
                // Handle control characters up front
                if (c == '\r') { continue; } // ignore CR (for CRLF)
                if (c == '\n') { y++; x = 0; continue; }

                if (!positionCharDict.TryGetValue((x, y), out var list))
                {
                    list = new List<LayerChar>();
                    positionCharDict[(x, y)] = list;
                }

                list.Add(new LayerChar(textLayer, c, x, y));
                x++;
            }
        }
    }

    private static void UpdateLayerStates(TextLayer textLayerJustRendered, List<TextLayer> textLayers, ConcurrentQueue<TextLayer> toRenderQueue)
    {
        foreach (var layer in textLayers.Where(x => x.State == TextLayerState.Rendered))
        {
            layer.State = TextLayerState.Initialized;
        }

        textLayerJustRendered.State = TextLayerState.Rendered;

        if (toRenderQueue.TryDequeue(out var dequeuedLayer))
        {
            if (dequeuedLayer != textLayerJustRendered)
            {
                throw new InvalidStateException(new { dequeuedLayer, textLayerJustRendered });
            }
        }
        else
        {
            throw new InvalidStateException(toRenderQueue);
        }
    }

    private static void AppendLayerToRenderToStringBuilder(Dictionary<(int X, int Y), List<LayerChar>> positionCharDict, TextLayer textLayerToRender, StringBuilder sb)
    {
        int maxX = positionCharDict.Max(x => x.Key.X);
        int maxY = positionCharDict.Max(y => y.Key.Y);

        //TODO get number of rows for both

        //TODO get length of each row for both

        //TODO use the numbers to iterate exacly

        for (int y = 0; y <= maxY; y++)
        {
            //too much logic in this block
            for (int x = 0; x <= maxX; x++)
            {
                if (positionCharDict.TryGetValue((x, y), out var layerCharsAtPosition))
                {
                    var toRenderChars = layerCharsAtPosition.Where(x => x.TextLayer == textLayerToRender);
                    var renderedChars = layerCharsAtPosition.Where(x => x.TextLayer.State == TextLayerState.Rendered);

                    if (toRenderChars.Count() > 1)
                    {
                        throw new InvalidStateException(toRenderChars);
                    }

                    if (renderedChars.Count() > 1)
                    {
                        throw new InvalidStateException(renderedChars);
                    }

                    var toRenderChar = toRenderChars.FirstOrDefault();
                    var renderedChar = renderedChars.FirstOrDefault();

                    bool noCharExist = renderedChar == default && toRenderChar == default;

                    if (noCharExist)
                    {
                        sb.Append(Environment.NewLine);
                        break;
                    }

                    bool onlyRenderedCharExist = renderedChar != default && toRenderChar == default;

                    if (onlyRenderedCharExist)
                    {
                        sb.Append(' ');
                        continue;
                    }

                    bool onlyToRenderCharExist = renderedChar == default && toRenderChar != default;

                    if (onlyToRenderCharExist)
                    {
                        sb.Append(toRenderChar!.Char);
                        continue;
                    }

                    bool bothCharsExist = renderedChar != default && toRenderChar != default;

                    if (bothCharsExist)
                    {
                        sb.Append(renderedChar!.Char);
                        continue;
                    }

                    throw new InvalidStateException(new { x, y, layerCharsAtPosition, positionCharDict });
                }
            }
        }
    }

    private void RenderToConsole(StringBuilder sb, ConsoleColor foregroundColor)
    {
        Console.ForegroundColor = foregroundColor;
        _consoleAdapter.SetCursorPosition(0, 0);
        Console.WriteLine(sb.ToString());
        _consoleAdapter.SetCursorPosition(0, 0);
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
    Clear
}

internal class ConsoleAdapter
{
    private Action<int, int> _setCursorPosition = Console.SetCursorPosition;
    private Action _clear = Console.Clear;
    public ConsoleAdapter() { }

    //test constructor
    internal ConsoleAdapter(Action<int, int> setCursorPosition, Action clear)
    {
        _setCursorPosition = setCursorPosition;
        _clear = clear;
    }
    public void SetCursorPosition(int x, int y) => _setCursorPosition?.Invoke(x, y);
    public void Clear() => _clear?.Invoke();
}
