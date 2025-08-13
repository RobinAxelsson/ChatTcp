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
        public void MapTextLayerCharsToPositionDictionary_ShouldHandleNewlines()
        {
            // Arrange
            var textLayer = new TextLayer(ConsoleColor.White, "01\r\n23\n4");
            var positionCharDict = new Dictionary<(int X, int Y), List<LayerChar>>();

            // Act
            RenderSystem.MapTextLayerCharsToPositionDictionary(positionCharDict, new List<TextLayer> { textLayer });

            // Assert
            // Expected positions: 
            // (0,0) => '0'
            // (0,1) => '3'
            var expected = new List<(int X, int Y, char C)>
            {
                (0, 0, '0'),
                (1, 0, '1'),
                (0, 1, '2'),
                (1, 1, '3'),
                (0, 2, '4'),
            };

            Assert.Equal(expected.Count, positionCharDict.Count);

            foreach (var (x, y, c) in expected)
            {
                Assert.True(positionCharDict.ContainsKey((x, y)), $"Missing key ({x},{y})");
                var list = positionCharDict[(x, y)];
                Assert.Single(list); // only one LayerChar at that coordinate
                Assert.Equal(c, list[0].Char);
                Assert.Equal(x, list[0].X);
                Assert.Equal(y, list[0].Y);
                Assert.Same(textLayer, list[0].TextLayer);
            }
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

        [Fact]
        public void Tick_WithNoQueuedLayers_ShouldNotThrowException()
        {
            var renderSystem = CreateSut(new List<TextLayer>());
            renderSystem.Tick();
        }

        [Theory]
        [InlineData("Single line text")]
        [InlineData("Multi\nLine\nText")]
        [InlineData("Text with\ttabs")]
        [InlineData("")]
        [InlineData("A")]
        public void RenderSystem_ShouldHandleVariousTextFormats(string text)
        {
            var textLayer = new TextLayer(ConsoleColor.White, text);
            var renderSystem = CreateSut(new List<TextLayer> { textLayer });

            renderSystem.RequestRender(textLayer);
            renderSystem.Tick();

            Assert.Equal(TextLayerState.Rendered, textLayer.State);
            Assert.Equal(text, _consoleOutput.ToString());
        }
    }
}
