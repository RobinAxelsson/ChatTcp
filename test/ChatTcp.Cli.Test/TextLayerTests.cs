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
            Assert.NotNull(layer.Rows);
            Assert.Equal(2, layer.Rows.Count);
            Assert.Equal(new List<char> { 'A', 'B' }, layer.Rows[0]);
            Assert.Equal(new List<char> { 'C', 'D' }, layer.Rows[1]);
        }

        [Fact]
        public void Ctor_MapsTextToRows_SingleLine()
        {
            var layer = new TextLayer(ConsoleColor.White, "XYZ");

            Assert.NotNull(layer.Rows);
            Assert.Single(layer.Rows);
            Assert.Equal(new List<char> { 'X', 'Y', 'Z' }, layer.Rows[0]);
        }

        [Fact]
        public void Ctor_MapsTextToRows_EmptyString()
        {
            var layer = new TextLayer(ConsoleColor.White, "");

            Assert.NotNull(layer.Rows);
            // Represent one empty row rather than no rows
            Assert.Single(layer.Rows);
            Assert.Empty(layer.Rows[0]);
        }

        [Fact]
        public void Ctor_MapsTextToRows_MultipleEmptyLines()
        {
            var layer = new TextLayer(ConsoleColor.White, "\n\n");

            Assert.NotNull(layer.Rows);
            Assert.Equal(3, layer.Rows.Count); // "" , "" , ""
            Assert.All(layer.Rows, row => Assert.Empty(row));
        }
    }
}
