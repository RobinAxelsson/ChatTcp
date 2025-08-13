using Xunit;

namespace ChatTcp.Cli.Tests
{
    public class TextLayerTests
    {
        [Fact]
        public void Ctor_MapsTextToRows_TwoLines()
        {
            var layer = new TextLayer(ConsoleColor.White, "AB\nCD");

            // Expect 2 rows: ['A','B'] and ['C','D']
            Assert.NotNull(layer.LayerBuffer);
            Assert.Equal(2, layer.LayerBuffer.Count);
            Assert.Equal(new List<char> { 'A', 'B' }, layer.LayerBuffer[0]);
            Assert.Equal(new List<char> { 'C', 'D' }, layer.LayerBuffer[1]);
        }

        [Fact]
        public void Ctor_MapsTextToRows_SingleLine()
        {
            var layer = new TextLayer(ConsoleColor.White, "XYZ");

            Assert.NotNull(layer.LayerBuffer);
            Assert.Single(layer.LayerBuffer);
            Assert.Equal(new List<char> { 'X', 'Y', 'Z' }, layer.LayerBuffer[0]);
        }

        [Fact]
        public void Ctor_MapsTextToRows_EmptyString()
        {
            var layer = new TextLayer(ConsoleColor.White, "");

            Assert.NotNull(layer.LayerBuffer);
            // Represent one empty row rather than no rows
            Assert.Single(layer.LayerBuffer);
            Assert.Empty(layer.LayerBuffer[0]);
        }

        [Fact]
        public void Ctor_MapsTextToRows_MultipleEmptyLines()
        {
            var layer = new TextLayer(ConsoleColor.White, "\n\n");

            Assert.NotNull(layer.LayerBuffer);
            Assert.Equal(3, layer.LayerBuffer.Count); // "" , "" , ""
            Assert.All(layer.LayerBuffer, row => Assert.Empty(row));
        }
    }
}
