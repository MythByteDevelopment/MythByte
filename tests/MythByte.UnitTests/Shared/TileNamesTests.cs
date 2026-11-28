using MythByte.Shared.Maps;

namespace MythByte.UnitTests.Shared;

public class TileNamesTests
{
    [Fact]
    public void Format_UnsortedNames_WritesSortedEntries()
    {
        var names = new Dictionary<int, string> { [3] = "tree_pine", [0] = "tree_birch" };

        var value = TileNames.Format(names);

        Assert.Equal("0=tree_birch;3=tree_pine", value);
    }

    [Fact]
    public void Format_NameWithSeparator_ThrowsArgumentException()
    {
        var names = new Dictionary<int, string> { [0] = "a;b" };

        Assert.Throws<ArgumentException>(() => TileNames.Format(names));
    }

    [Fact]
    public void Parse_ValidValue_ReadsEntries()
    {
        var names = TileNames.Parse("0=tree_birch;3=tree_pine");

        Assert.Equal("tree_birch", names[0]);
        Assert.Equal("tree_pine", names[3]);
    }

    [Theory]
    [InlineData("tree")]
    [InlineData("x=tree")]
    [InlineData("-1=tree")]
    [InlineData("1=")]
    public void Parse_InvalidEntry_ThrowsFormatException(string value)
    {
        Assert.Throws<FormatException>(() => TileNames.Parse(value));
    }

    [Fact]
    public void ByName_DuplicateName_UsesLowestId()
    {
        var names = new Dictionary<int, string> { [4] = "grass", [1] = "grass" };

        var byName = TileNames.ByName(names);

        Assert.Equal(1, byName["grass"]);
    }
}
