namespace SearchOptimizationBE.Models;

public class Document
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DocumentTypeId { get; set; }
    public DocumentType? DocumentType { get; set; }
    public int UploadedById { get; set; }
    public User? UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
    public int FileSizeKb { get; set; }
}
