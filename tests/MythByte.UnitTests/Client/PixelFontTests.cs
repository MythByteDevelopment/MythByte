using MythByte.Client.Rendering;

namespace MythByte.UnitTests.Client;

public class PixelFontTests
{
    [Theory]
    [InlineData("A", 1, 5)]
    [InlineData("AB", 1, 11)]
    [InlineData("AB", 4, 44)]
    public void MeasureWidth_Text_ReturnsGlyphsPlusSpacing(string text, int scale, int expected)
    {
        var result = PixelFont.MeasureWidth(text, scale);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MeasureWidth_EmptyText_ReturnsZero()
    {
        var result = PixelFont.MeasureWidth("", 4);

        Assert.Equal(0, result);
    }

    [Fact]
    public void GetGlyph_LowercaseLetter_ReturnsUppercaseGlyph()
    {
        var result = PixelFont.GetGlyph('v');

        Assert.Same(PixelFont.GetGlyph('V'), result);
    }

    [Fact]
    public void GetGlyph_UnknownCharacter_ReturnsSpace()
    {
        var result = PixelFont.GetGlyph('~');

        Assert.Same(PixelFont.GetGlyph(' '), result);
    }

    [Fact]
    public void GetGlyph_AllSupportedCharacters_HaveFullSize()
    {
        var characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-:>/ ";

        var glyphs = characters.Select(PixelFont.GetGlyph).ToList();

        Assert.All(glyphs, glyph =>
        {
            Assert.Equal(PixelFont.GlyphHeight, glyph.Length);
            Assert.All(glyph, row => Assert.Equal(PixelFont.GlyphWidth, row.Length));
        });
    }
}
