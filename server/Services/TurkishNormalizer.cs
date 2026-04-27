using System.Globalization;
using System.Text;

namespace SearchOptimizationBE.Services;

public static class TurkishNormalizer
{
    private static readonly CultureInfo TrCulture = new("tr-TR");

    public static string Normalize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var lowered = input.ToLower(TrCulture);
        var sb = new StringBuilder(lowered.Length);
        foreach (var ch in lowered)
        {
            sb.Append(ch switch
            {
                'ç' => 'c',
                'ğ' => 'g',
                'ı' => 'i',
                'i' => 'i',
                'ö' => 'o',
                'ş' => 's',
                'ü' => 'u',
                _ => ch
            });
        }
        return sb.ToString().Trim();
    }
}
