using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ChatTcp.Cli.Tests
{
    public class RenderSystemTests : IDisposable
    {
        private readonly StringWriter _consoleOutput;
        private readonly TextWriter _originalConsoleOut;
        public RenderSystemTests(ITestOutputHelper testOutputHelper)
        {
            _originalConsoleOut = Console.Out;
            _consoleOutput = new StringWriter();
            Console.SetOut(_consoleOutput);
            //_consoleOutput.GetStringBuilder().Clear();
        }

        public void Dispose()
        {
            Console.SetOut(_originalConsoleOut);
            _consoleOutput.Dispose();
        }

        private RenderSystem CreateSut(List<TextLayer> textLayers = null)
        {
            //fix console handle is invalid with ConsoleAdapter stub
            return new RenderSystem(textLayers ?? new List<TextLayer>(), new ConsoleAdapter((_, _) => { }, () => { }));
        }

        [Fact]
        public void Tick_WithClearState_ShouldResetRenderedLayersToInitialized()
        {
            var textLayer1 = new TextLayer(ConsoleColor.White, "Layer1") { State = TextLayerState.Rendered };
            var textLayer2 = new TextLayer(ConsoleColor.White, "Layer2") { State = TextLayerState.Rendered };

            var renderSystem = CreateSut(new List<TextLayer> { textLayer1, textLayer2 });

            renderSystem.RequestClear();
            renderSystem.Tick();

            Assert.Equal(TextLayerState.Initialized, textLayer1.State);
            Assert.Equal(TextLayerState.Initialized, textLayer2.State);
        }

        [Fact]
        public void Tick_WithRenderRequest_ShouldProcessQueuedLayer()
        {
            var textLayer = new TextLayer(ConsoleColor.Cyan, "Hello");
            var renderSystem = CreateSut(new List<TextLayer> { textLayer });

            renderSystem.RequestRender(textLayer);
            renderSystem.Tick();

            Assert.Equal(TextLayerState.Rendered, textLayer.State);
        }

        [Fact]
        public void Tick_WithMultipleLayersInQueue_ShouldProcessOneAtATime()
        {
            var textLayer1 = new TextLayer(ConsoleColor.Red, "First");
            var textLayer2 = new TextLayer(ConsoleColor.Blue, "Second");
            var renderSystem = CreateSut(new List<TextLayer> { textLayer1, textLayer2 });

            renderSystem.RequestRender(textLayer1);
            renderSystem.RequestRender(textLayer2);

            renderSystem.Tick();
            Assert.Equal(TextLayerState.Rendered, textLayer1.State);
            Assert.Equal(TextLayerState.Initialized, textLayer2.State);

            renderSystem.Tick();
            Assert.Equal(TextLayerState.Initialized, textLayer1.State);
            Assert.Equal(TextLayerState.Rendered, textLayer2.State);
        }

        [Fact]
        public void Tick_WithEmptyTextLayer_ShouldNotThrowException()
        {
            var textLayer = new TextLayer(ConsoleColor.White, "");
            var renderSystem = CreateSut(new List<TextLayer> { textLayer });

            renderSystem.RequestRender(textLayer);
            renderSystem.Tick();

            Assert.Equal(TextLayerState.Rendered, textLayer.State);
        }

        [Theory]
        [InlineData("Single line text")]
        [InlineData("Multi\nLine\nText")]
        [InlineData("Text with\ttabs")]
        [InlineData("")]
        [InlineData("A")]
        public void Tick_TextSerializedAndDeserialized_ShouldMatchInput(string text)
        {
            var textLayer = new TextLayer(ConsoleColor.White, text);
            var renderSystem = CreateSut(new List<TextLayer> { textLayer });

            renderSystem.RequestRender(textLayer);
            renderSystem.Tick();

            Assert.Equal(text.ReplaceLineEndings(), _consoleOutput.ToString());
        }

        [Fact]
        public void Tick_TwoTextLayersAB_ShouldWriteOrOverwrite()
        {
            var oldTextLayer = new TextLayer(ConsoleColor.White, "A");
            var newTextLayer = new TextLayer(ConsoleColor.White, "B");
            var renderSystem = CreateSut(new List<TextLayer> { oldTextLayer, newTextLayer });

            renderSystem.RequestRender(oldTextLayer);
            renderSystem.Tick();

            Assert.Equal(TextLayerState.Rendered, oldTextLayer.State);
            Assert.Equal(TextLayerState.Initialized, newTextLayer.State);
            Assert.Equal("A", _consoleOutput.ToString());

            renderSystem.RequestRender(newTextLayer);
            renderSystem.Tick();

            Assert.Equal(TextLayerState.Initialized, oldTextLayer.State);
            Assert.Equal(TextLayerState.Rendered, newTextLayer.State);
            Assert.Equal("B", _consoleOutput.ToString());
        }
    }
}
