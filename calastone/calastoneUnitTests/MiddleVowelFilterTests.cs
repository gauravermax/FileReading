using Calastone.Filters;
using Xunit;

namespace CalastoneTests;

public class MiddleVowelFilterTests
{
    private readonly MiddleVowelFilter _filter = new();

    [Fact]
    public void Apply_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _filter.Apply(null!).ToList());
    }

    [Fact]
    public void Apply_OddLength_MiddleVowel_IsRemoved()
    {
        // "currently" (len 9) -> middle index 4 = 'e' -> removed
        // "clean" (len 5) -> middle index 2 = 'e' -> removed
        var result = _filter.Apply(new[] { "currently", "clean" }).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_OddLength_NoMiddleVowel_IsKept()
    {
        // "the" (len 3) -> middle index 1 = 'h' -> kept
        var result = _filter.Apply(new[] { "the" }).ToList();
        Assert.Equal(new[] { "the" }, result);
    }

    [Fact]
    public void Apply_EvenLength_MiddleVowel_IsRemoved()
    {
        // "what" (len 4) -> middle indices 1,2 = 'h','a' -> 'a' is vowel -> removed
        var result = _filter.Apply(new[] { "what" }).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Apply_EvenLength_NoMiddleVowel_IsKept()
    {
        // "rather" (len 6) -> middle indices 2,3 = 't','h' -> no vowels -> kept
        var result = _filter.Apply(new[] { "rather" }).ToList();
        Assert.Equal(new[] { "rather" }, result);
    }

    [Fact]
    public void Apply_MixedWords_FiltersCorrectly()
    {
        var result = _filter.Apply(new[] { "the", "currently", "rather", "clean", "fox" }).ToList();
        Assert.Equal(new[] { "the", "rather" }, result);
    }

    [Fact]
    public void Apply_UpperCaseVowelInMiddle_IsRemoved()
    {
        // "cAr" (len 3) -> middle 'A' -> vowel -> removed (case insensitive)
        var result = _filter.Apply(new[] { "cAr" }).ToList();
        Assert.Empty(result);
    }
}
