
using calastone.Pipeline;
using Calastone.Filters;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CalastoneTests;
public class ParallelSequentialConsistancyTests
{
    private readonly ITextFilter[] _filters;
    private readonly ILogger<TextFilterPipeline> _logger;
    private readonly ITextFilterPipeline _pipeline;
    public ParallelSequentialConsistancyTests()
    {
        _filters = new ITextFilter[]
        {
                new MiddleVowelFilter(),
                new ShortWordFilter(),
                new LetterTFilter('t')
        };
        _logger = new Mock<ILogger<TextFilterPipeline>>().Object;
        _pipeline = new TextFilterPipeline(_filters, _logger);
    }

   
    //private string ApplyParallel(string input)
    //{
    //    if (string.IsNullOrWhiteSpace(input)) return string.Empty;
    //    string[] words = input.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries);
    //    var filterTasks = _filters.Select(filter => Task.Run(() =>
    //    new HashSet<string>(filter.Apply(words))
    //    )).ToArray();
    //    Task.WaitAll(filterTasks);
    //    var passedWords = filterTasks
    //    .Select(t => t.Result)
    //    .Aggregate((current, next) => { current.IntersectWith(next); return current; });
    //    return string.Join(" ", words.Where(w => passedWords.Contains(w)));
    //}

    //   private string ApplySequential(string input)
    //{
    //    if (string.IsNullOrWhiteSpace(input)) return string.Empty;
    //    string[] words = input.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries);
    //    IEnumerable<string> currentWords = words;
    //    foreach (var filter in _filters)
    //    {
    //        currentWords = filter.Apply(currentWords).ToArray();
    //    }
    //    return string.Join(" ", currentWords);
    //}

    [Fact]
    public void ParallelAndSequential_SameInput_SameResult()
    {
        string input = "The quick brown fox jumps over the lazy dog and currently runs rather fast";
        string parallelResult = _pipeline.Apply(input);
        string sequentialResult = _pipeline.Apply(input,true);
        Assert.Equal(sequentialResult, parallelResult);
    }

    [Fact]
    public void ParallelAndSequential_SingleWord_SameResult()
    {
        string input = "fox";
        Assert.Equal(_pipeline.Apply(input, true), _pipeline.Apply(input));
    }
    [Fact]
    public void ParallelAndSequential_AllWordsFiltered_SameResult()
    {
        // "the" → removed by LetterTFilter, "it" → removed by ShortWordFilter & LetterTFilter
        string input = "the it at";
        Assert.Equal(_pipeline.Apply(input, true), _pipeline.Apply(input));
    }
    [Fact]
    public void ParallelAndSequential_NoWordsFiltered_SameResult()
    {
        // "brown" passes all 3 filters: length >= 3, no 't', no middle vowel
        string input = "brown";
        Assert.Equal(_pipeline.Apply(input, true), _pipeline.Apply(input));
    }
    [Fact]
    public void ParallelAndSequential_EmptyInput_SameResult()
    {
        string input = "";
        Assert.Equal(_pipeline.Apply(input, true), _pipeline.Apply(input));
    }
    [Fact]
    public void ParallelAndSequential_LargeInput_SameResult()
    {
        // Repeat to create a larger dataset
        string baseLine = "The quick brown fox jumps over the lazy dog and currently runs rather fast";
        string input = string.Join(" ", Enumerable.Repeat(baseLine, 100));
        Assert.Equal(_pipeline.Apply(input, true), _pipeline.Apply(input));
    }

    [Theory]
    [InlineData("hello world foo bar baz")]
    [InlineData("testing multiple strange words here")]
    [InlineData("a b c d e f g h i j k l m")]
    public void ParallelAndSequential_VariousInputs_SameResult(string input)
    {
        Assert.Equal(_pipeline.Apply(input, true), _pipeline.Apply(input));
    }
   
}