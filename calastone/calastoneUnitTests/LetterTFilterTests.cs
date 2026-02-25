using Calastone.Filters;
using Xunit;

namespace CalastoneTests;

public class LetterTFilterTests
{
    private readonly LetterTFilter _filter = new('t');

    [Fact]
    public void Apply_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _filter.Apply(null!).ToList());
    }

    [Fact]
    public void Apply_WordsWithT_AreRemoved()
    {
        var result = _filter.Apply(new[] { "the", "cat", "sit" }).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_WordsWithoutT_AreKept()
    {
        var result = _filter.Apply(new[] { "fox", "brown", "jumps" }).ToList();
        Assert.Equal(new[] { "fox", "brown", "jumps" }, result);
    }

    [Fact]
    public void Apply_CaseInsensitive_UppercaseTRemoved()
    {
        var result = _filter.Apply(new[] { "The", "TOTAL" }).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_MixedWords_FiltersCorrectly()
    {
        var result = _filter.Apply(new[] { "the", "quick", "brown", "fox", "fast" }).ToList();
        Assert.Equal(new[] { "quick", "brown", "fox" }, result);
    }
}
