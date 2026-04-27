namespace SearchOptimizationBE.Models;

public class DocumentContentHash
{
    public Guid DocumentId { get; set; }
    public Document? Document { get; set; }
    public byte[] ContentSha256 { get; set; } = Array.Empty<byte>();
    public string NormalizedTitle { get; set; } = string.Empty;
}
