using Calastone.Filters;
using Xunit;

namespace CalastoneTests;

public class ShortWordFilterTests
{
    private readonly ShortWordFilter _filter = new();

    [Fact]
    public void Apply_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _filter.Apply(null!).ToList());
    }

    [Fact]
    public void Apply_ShortWords_AreRemoved()
    {
        // Words with length < 3 should be removed
        var result = _filter.Apply(new[] { "a", "is", "I" }).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_ThreeOrMoreChars_AreKept()
    {
        var result = _filter.Apply(new[] { "the", "fox", "currently" }).ToList();
        Assert.Equal(new[] { "the", "fox", "currently" }, result);
    }

    [Fact]
    public void Apply_MixedLengths_FiltersCorrectly()
    {
        var result = _filter.Apply(new[] { "a", "is", "the", "fox", "currently" }).ToList();
        Assert.Equal(new[] { "the", "fox", "currently" }, result);
    }

    [Fact]
    public void Apply_BoundaryLength_ThreeCharsKept()
    {
        // Exactly 3 chars = boundary -> should be kept
        var result = _filter.Apply(new[] { "abc" }).ToList();
        Assert.Single(result);
        Assert.Equal("abc", result[0]);
    }
}
