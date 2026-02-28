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

    [Fact]
    public void Apply_OneFilterThrows_OtherFiltersStillApplied_Serial()
    {
        var workingFilter1 = new ShortWordFilter();
        var workingFilter2 = new LetterTFilter('t');
        var failingFilter = new Mock<ITextFilter>();
        failingFilter.Setup(f => f.Name).Returns("FailingFilter");
        failingFilter.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new InvalidOperationException("Something went wrong"));
        var filters = new ITextFilter[] { workingFilter1, failingFilter.Object, workingFilter2 };
        
        var pipeline = CreatePipeline(filters);

        string result = pipeline.Apply("The quick brown fox", true);
        // "The" removed by LetterTFilter, failing filter didn't block others
        Assert.DoesNotContain("The", result.Split(' '));
        Assert.Contains("quick", result.Split(' '));
        Assert.Contains("brown", result.Split(' '));
        Assert.Contains("fox", result.Split(' '));
    }

    [Fact]
    public void Apply_AllFiltersThrow_ReturnsOriginalText_Serial()
    {
        var failing1 = new Mock<ITextFilter>();
        failing1.Setup(f => f.Name).Returns("Failing1");
        failing1.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new Exception("fail 1"));
        var failing2 = new Mock<ITextFilter>();
        failing2.Setup(f => f.Name).Returns("Failing2");
        failing2.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new Exception("fail 2"));
        var filters = new ITextFilter[] { failing1.Object, failing2.Object };
        
        var pipeline = CreatePipeline(filters);

        string result = pipeline.Apply("The quick brown fox", true);
        //  all filters failed, so all words kept
        Assert.Equal("The quick brown fox", result);
    }
    [Fact]
    public void Apply_OneFilterThrows_FailureIsLogged_Serial()
    {
        var failingFilter = new Mock<ITextFilter>();
        failingFilter.Setup(f => f.Name).Returns("BrokenFilter");
        failingFilter.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new InvalidOperationException("Broken"));
        var mockLogger = new Mock<ILogger<TextFilterPipeline>>();
        var pipeline = new TextFilterPipeline(
            new ITextFilter[] { failingFilter.Object },
            mockLogger.Object);
        
        pipeline.Apply("hello world", true);
        // verify error was logged
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("BrokenFilter")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    [Fact]
    public void Apply_OneFilterThrows_OtherFiltersStillApplied_Parallel()
    {
        var workingFilter1 = new ShortWordFilter();
        var workingFilter2 = new LetterTFilter('t');
        var failingFilter = new Mock<ITextFilter>();
        failingFilter.Setup(f => f.Name).Returns("FailingFilter");
        failingFilter.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new InvalidOperationException("Something went wrong"));
        var filters = new ITextFilter[] { workingFilter1, failingFilter.Object, workingFilter2 };
        
        var pipeline = CreatePipeline(filters);

        string result = pipeline.Apply("The quick brown fox");
        // "The" removed by LetterTFilter, failing filter didn't block others
        Assert.DoesNotContain("The", result.Split(' '));
        Assert.Contains("quick", result.Split(' '));
        Assert.Contains("brown", result.Split(' '));
        Assert.Contains("fox", result.Split(' '));
    }
    [Fact]
    public void Apply_AllFiltersThrow_ReturnsOriginalText_Parallell()
    {
        var failing1 = new Mock<ITextFilter>();
        failing1.Setup(f => f.Name).Returns("Failing1");
        failing1.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new Exception("fail 1"));
        var failing2 = new Mock<ITextFilter>();
        failing2.Setup(f => f.Name).Returns("Failing2");
        failing2.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new Exception("fail 2"));
        var filters = new ITextFilter[] { failing1.Object, failing2.Object };
       
        var pipeline = CreatePipeline(filters);

        string result = pipeline.Apply("The quick brown fox");
        //  all filters failed, so all words kept
        Assert.Equal("The quick brown fox", result);
    }
    [Fact]
    public void Apply_OneFilterThrows_FailureIsLogged_Parallel()
    {
        var failingFilter = new Mock<ITextFilter>();
        failingFilter.Setup(f => f.Name).Returns("BrokenFilter");
        failingFilter.Setup(f => f.Apply(It.IsAny<IEnumerable<string>>()))
            .Throws(new InvalidOperationException("Broken"));
        var mockLogger = new Mock<ILogger<TextFilterPipeline>>();
        var pipeline = new TextFilterPipeline(
            new ITextFilter[] { failingFilter.Object },
            mockLogger.Object);

        pipeline.Apply("hello world");

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("BrokenFilter")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
