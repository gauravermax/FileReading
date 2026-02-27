namespace calastone.Pipeline;

/// <summary>
/// pipeline for running filters
/// </summary>
public interface ITextFilterPipeline
{
    /// <summary>
    /// Applies all filters in default parallel to the given text.
    /// Words are split by whitespace. Only words that pass every filter are kept.
    /// </summary>
    /// <param name="text">The input text to filter.</param>
    /// <param name="useSequenceFlow">use sequecial flow or parallel flow</param>
    /// <returns>The filtered text with only words that passed all filters.</returns>
    string Apply(string text, bool useSequenceFlow = false);
}

