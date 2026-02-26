using Microsoft.Extensions.Logging;
using System.Linq;

namespace Calastone.Filters;

/// <summary>
/// Runs multiple text filters in parallel and combines results.
/// Only words that pass ALL filters are kept in the final output.
/// </summary>
public class TextFilterPipeline
{
    private readonly IReadOnlyList<ITextFilter> _filters;
    private readonly ILogger<TextFilterPipeline> _logger;

    public TextFilterPipeline(IEnumerable<ITextFilter> filters, ILogger<TextFilterPipeline> logger)
    {
        _filters = filters?.ToList().AsReadOnly()
            ?? throw new ArgumentNullException(nameof(filters));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Applies all filters in parallel to the given text.
    /// Words are split by whitespace. Only words that pass every filter are kept.
    /// </summary>
    /// <param name="text">The input text to filter.</param>
    /// <returns>The filtered text with only words that passed all filters.</returns>
    public string Apply(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            _logger.LogWarning("Input text is null or empty. Returning empty string.");
            return string.Empty;
        }

        string[] words = text.Split((char[])null!, StringSplitOptions.RemoveEmptyEntries);
        _logger.LogInformation("Starting filter pipeline with {WordCount} words and {FilterCount} filters.",
            words.Length, _filters.Count);

        // If no filters configured, return all words as-is
        if (_filters.Count == 0)
        {
            _logger.LogInformation("No filters configured. Returning all words.");
            return string.Join(" ", words);
        }

        // Adding cancellation token for 5 sec 
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Run each filter in parallel — each filter independently decides which words to keep
        var filterTasks = _filters.Select(filter => Task.Run(() =>
        {
            try
            {
                _logger.LogDebug("Applying filter: {FilterName}", filter.Name);
                var result = new HashSet<string>(filter.Apply(words));
                _logger.LogDebug("Filter {FilterName} kept {KeptCount} of {TotalCount} words.",
                    filter.Name, result.Count, words.Length);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying filter {FilterName}. Keeping all words for this filter.",
                    filter.Name);
                return new HashSet<string>(words);
            }
        }, cts.Token)).ToArray();



        Task.WaitAll(filterTasks);

        // Intersect results: only keep words that passed ALL filters (preserve original order)
        var passedWords = filterTasks
            .Select(t => t.Result)
            .Aggregate((current, next) => { current.IntersectWith(next); return current; });

        var result = words.Where(w => passedWords.Contains(w)).ToArray();

        _logger.LogInformation("Filter pipeline complete. {KeptCount} of {TotalCount} words remaining.",
            result.Length, words.Length);

        return string.Join(" ", result);
    }
    }
