namespace Calastone.Filters;

/// <summary>
/// Removes words that have a length less than 3 characters.
/// </summary>
public class ShortWordFilter : ITextFilter
{
    public string Name => "ShortWordFilter";

    public IEnumerable<string> Apply(IEnumerable<string> words)
    {
        if (words == null) throw new ArgumentNullException(nameof(words));

        return words.Where(word => word.Length >= 3);
    }
}
