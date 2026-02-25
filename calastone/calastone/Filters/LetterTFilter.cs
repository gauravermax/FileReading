namespace Calastone.Filters;

/// <summary>
/// Removes words that contain the letter 't' (case-insensitive).
/// </summary>
public class LetterTFilter : ITextFilter
{
    private readonly char _targetChar;
    public string Name => "LetterTFilter";

    // Inject the specific letter we want to filter
    public LetterTFilter(char targetChar)
    {
        _targetChar = targetChar;
    }

    public IEnumerable<string> Apply(IEnumerable<string> words)
    {
        if (words == null) throw new ArgumentNullException(nameof(words));

        return words.Where(word => !word.Contains(_targetChar, StringComparison.OrdinalIgnoreCase));
    }
}
