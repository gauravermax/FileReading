using Microsoft.Extensions.Logging;
using Moq;
using Calastone.Filters;
using Xunit;
using calastone.Pipeline;

namespace CalastoneTests;

public class TextFilterPipelineTests
{
    private readonly Mock<ILogger<TextFilterPipeline>> _loggerMock = new();

    private TextFilterPipeline CreatePipeline(params ITextFilter[] filters)
    {
        return new TextFilterPipeline(filters, _loggerMock.Object);
    }

    [Fact]
    public void Constructor_NullFilters_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TextFilterPipeline(null!, _loggerMock.Object));
    }

    [Fact]
    public void Apply_EmptyOrNullText_ReturnsEmptyString()
    {
        var pipeline = CreatePipeline(new ShortWordFilter());
        Assert.Equal(string.Empty, pipeline.Apply(null!));
        Assert.Equal(string.Empty, pipeline.Apply(""));
        Assert.Equal(string.Empty, pipeline.Apply("   "));
    }

    [Fact]
    public void Apply_NoFilters_ReturnsOriginalWords()
    {
        var pipeline = CreatePipeline();
        var result = pipeline.Apply("hello world foo bar");
        Assert.Equal("hello world foo bar", result);
    }

    [Fact]
    public void Apply_AllThreeFilters_CombinedFiltering()
    {
        var pipeline = CreatePipeline(
            new MiddleVowelFilter(),
            new ShortWordFilter(),
            new LetterTFilter('t'));

        var result = pipeline.Apply("The quick brown fox jumps over the lazy dog and currently runs rather fast");
        // Only "jumps" and "and" survive all three filters
        Assert.Equal("jumps and", result);
    }

    [Fact]
    public void Apply_AllWordsFiltered_ReturnsEmptyString()
    {
        var pipeline = CreatePipeline(
            new MiddleVowelFilter(),
            new ShortWordFilter(),
            new LetterTFilter('t'));

        var result = pipeline.Apply("it at");
        Assert.Equal("", result);
    }

    [Fact]
    public void Apply_FilterThrowsException_GracefullyHandled()
    {
        var faultyFilter = new Mock<ITextFilter>();
        faultyFilter.Setup(f => f.Name).Returns("FaultyFilter");
        faultyFilter.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new InvalidOperationException("Test exception"));

        var pipeline = new TextFilterPipeline(
            new ITextFilter[] { faultyFilter.Object, new ShortWordFilter() },
            _loggerMock.Object);

        // Faulty filter falls back to keeping all words; ShortWordFilter still applies
        var result = pipeline.Apply("I am a fox in the den");
        Assert.Equal("fox the den", result);
    }

    [Fact]
    public void Apply_ParallelExecution_ProducesConsistentResults()
    {
        var pipeline = CreatePipeline(
            new MiddleVowelFilter(),
            new ShortWordFilter(),
            new LetterTFilter('t'));

        string input = "The quick brown fox jumps over the lazy dog and currently runs rather fast";

        // Run multiple times to verify parallel correctness
        for (int i = 0; i < 10; i++)
        {
            Assert.Equal("jumps and", pipeline.Apply(input));
        }
    }
}
