namespace Calastone.Filters;

/// <summary>
/// Removes words that contain a vowel in the middle position.
/// For odd-length words, the middle is a single character.
/// For even-length words, the middle consists of two characters.
/// </summary>
public class MiddleVowelFilter : ITextFilter
{
    private static readonly HashSet<char> Vowels = new(new[] { 'a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U' });

    public string Name => "MiddleVowelFilter";

    public IEnumerable<string> Apply(IEnumerable<string> words)
    {
        if (words == null) throw new ArgumentNullException(nameof(words));

        return words.Where(word => !HasMiddleVowel(word));
    }

    private static bool HasMiddleVowel(string word)
    {
        if (string.IsNullOrEmpty(word) || word.Length < 2)
            return false;

        int length = word.Length;

        if (length % 2 != 0)
        {
            // Odd length: single middle character
            int middleIndex = length / 2;
            return Vowels.Contains(word[middleIndex]);
        }
        else
        {
            // Even length: two middle characters
            int middleIndex1 = (length / 2) - 1;
            int middleIndex2 = length / 2;
            return Vowels.Contains(word[middleIndex1]) || Vowels.Contains(word[middleIndex2]);
        }
    }
}
