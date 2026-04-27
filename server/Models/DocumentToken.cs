namespace SearchOptimizationBE.Models;

public class DocumentToken
{
    public long Id { get; set; }
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public string Token { get; set; } = string.Empty;
    public byte Field { get; set; }
    public int Frequency { get; set; }
}

public static class TokenField
{
    public const byte Title = 1;
    public const byte Content = 2;
}
