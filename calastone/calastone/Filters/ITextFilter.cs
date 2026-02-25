namespace Calastone.Filters;

/// <summary>
/// Each filter implementation applies a specific filtering rule to a collection of words.
/// </summary>
public interface ITextFilter
{
    /// <summary>
    /// Gets the name of the filter for logging purposes.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Applies the filter to the given collection of words.
    /// </summary>
    /// <param name="words">The words to filter.</param>
    /// <returns>Words that pass the filter (i.e., words that should be kept).</returns>
    IEnumerable<string> Apply(IEnumerable<string> words);
}
