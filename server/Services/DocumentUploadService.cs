using SearchOptimizationBE.Data;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Services;

public record CreateDocumentRequest(string Title, string Content, int DocumentTypeId, int UploadedById, bool ConfirmDuplicate = false);

public record UploadResult(bool Success, Guid? DocumentId, DuplicateCheckResult? Duplicates, string? ErrorMessage);

public class DocumentUploadService
{
    private readonly AppDbContext _db;
    private readonly DuplicateDetectionService _duplicateService;

    public DocumentUploadService(AppDbContext db, DuplicateDetectionService duplicateService)
    {
        _db = db;
        _duplicateService = duplicateService;
    }

    public async Task<UploadResult> UploadAsync(CreateDocumentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Content))
            return new UploadResult(false, null, null, "Başlık ve içerik boş olamaz.");

        var typeExists = await _db.DocumentTypes.FindAsync(new object[] { request.DocumentTypeId }, ct);
        if (typeExists is null)
            return new UploadResult(false, null, null, "Geçersiz doküman tipi.");

        var userExists = await _db.Users.FindAsync(new object[] { request.UploadedById }, ct);
        if (userExists is null)
            return new UploadResult(false, null, null, "Geçersiz kullanıcı.");

        var duplicates = await _duplicateService.CheckAsync(request.Title, request.Content, ct);

        if (duplicates.ExactMatches.Count > 0 && !request.ConfirmDuplicate)
        {
            return new UploadResult(false, null, duplicates, "Aynı içerikli doküman zaten yüklenmiş. Yine de yüklemek için onay verin.");
        }

        var doc = new Document
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            DocumentTypeId = request.DocumentTypeId,
            UploadedById = request.UploadedById,
            UploadedAt = DateTime.UtcNow,
            FileSizeKb = request.Content.Length / 50 + 1
        };

        _db.Documents.Add(doc);

        _db.DocumentContentHashes.Add(new DocumentContentHash
        {
            DocumentId = doc.Id,
            ContentSha256 = ContentHasher.Sha256(doc.Title, doc.Content),
            NormalizedTitle = TurkishNormalizer.Normalize(doc.Title)
        });

        AddTokens(doc.Id, doc.Title, TokenField.Title);
        AddTokens(doc.Id, doc.Content, TokenField.Content);

        await _db.SaveChangesAsync(ct);

        return new UploadResult(true, doc.Id, duplicates.NearTitleMatches.Count > 0 || duplicates.ExactMatches.Count > 0 ? duplicates : null, null);
    }

    private void AddTokens(Guid documentId, string text, byte field)
    {
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var token in Tokenizer.Tokenize(text))
        {
            counts[token] = counts.GetValueOrDefault(token) + 1;
        }
        foreach (var (token, freq) in counts)
        {
            _db.DocumentTokens.Add(new DocumentToken
            {
                DocumentId = documentId,
                Token = token,
                Field = field,
                Frequency = freq
            });
        }
    }
}
