using System.Text.RegularExpressions;

namespace SearchOptimizationBE.Services;

public static class Tokenizer
{
    private static readonly Regex SplitRegex = new(@"[^\p{L}\p{Nd}]+", RegexOptions.Compiled);

    private static readonly HashSet<string> Stopwords = new(StringComparer.Ordinal)
    {
        "ve", "ile", "bir", "bu", "da", "de", "icin", "gibi", "ki", "mi", "mu", "mı", "mü",
        "ya", "ne", "cok", "hem", "ama", "fakat", "ancak", "en", "daha", "ise", "tum", "her",
        "bazi", "butun", "yine", "eger", "uzere", "kadar", "sonra", "once", "icinde",
        "baska", "diger", "ben", "sen", "biz", "siz", "onlar", "the", "of", "and", "to", "a", "in"
    };

    private static readonly string[] Suffixes =
    {
        "lardan", "lerden", "larda", "lerde", "lara", "lere", "lari", "leri",
        "lar", "ler",
        "nin", "nun", "nun", "nun",
        "dan", "den", "tan", "ten",
        "da", "de", "ta", "te", "ya", "ye",
        "in", "un", "un"
    };

    public static IEnumerable<string> Tokenize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) yield break;

        var normalized = TurkishNormalizer.Normalize(input);
        var parts = SplitRegex.Split(normalized);

        foreach (var part in parts)
        {
            if (part.Length < 2) continue;
            if (Stopwords.Contains(part)) continue;

            var stemmed = StripSuffix(part);
            if (stemmed.Length < 2) continue;

            yield return stemmed;
        }
    }

    private static string StripSuffix(string token)
    {
        foreach (var suffix in Suffixes)
        {
            if (token.Length - suffix.Length >= 4 && token.EndsWith(suffix, StringComparison.Ordinal))
            {
                return token[..^suffix.Length];
            }
        }
        return token;
    }
}
