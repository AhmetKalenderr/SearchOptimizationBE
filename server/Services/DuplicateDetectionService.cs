using Microsoft.EntityFrameworkCore;
using SearchOptimizationBE.Data;

namespace SearchOptimizationBE.Services;

public record DuplicateMatch(
    Guid DocumentId,
    string Title,
    string DocumentTypeName,
    string UploadedBy,
    DateTime UploadedAt,
    string MatchKind);

public record DuplicateCheckResult(
    IReadOnlyList<DuplicateMatch> ExactMatches,
    IReadOnlyList<DuplicateMatch> NearTitleMatches);

public class DuplicateDetectionService
{
    private readonly AppDbContext _db;

    public DuplicateDetectionService(AppDbContext db) => _db = db;

    public async Task<DuplicateCheckResult> CheckAsync(string title, string content, CancellationToken ct)
    {
        var hash = ContentHasher.Sha256(title, content);
        var normalizedTitle = TurkishNormalizer.Normalize(title);

        var exact = await _db.DocumentContentHashes
            .AsNoTracking()
            .Where(h => h.ContentSha256 == hash)
            .Join(_db.Documents.Include(d => d.DocumentType).Include(d => d.UploadedBy),
                h => h.DocumentId,
                d => d.Id,
                (h, d) => new DuplicateMatch(
                    d.Id,
                    d.Title,
                    d.DocumentType!.Name,
                    d.UploadedBy!.FullName,
                    d.UploadedAt,
                    "exact"))
            .ToListAsync(ct);

        var nearTitle = string.IsNullOrWhiteSpace(normalizedTitle)
            ? new List<DuplicateMatch>()
            : await _db.DocumentContentHashes
                .AsNoTracking()
                .Where(h => h.NormalizedTitle == normalizedTitle && h.ContentSha256 != hash)
                .Join(_db.Documents.Include(d => d.DocumentType).Include(d => d.UploadedBy),
                    h => h.DocumentId,
                    d => d.Id,
                    (h, d) => new DuplicateMatch(
                        d.Id,
                        d.Title,
                        d.DocumentType!.Name,
                        d.UploadedBy!.FullName,
                        d.UploadedAt,
                        "near-title"))
                .ToListAsync(ct);

        return new DuplicateCheckResult(exact, nearTitle);
    }
}
