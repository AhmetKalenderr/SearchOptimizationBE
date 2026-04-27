using Microsoft.EntityFrameworkCore;
using SearchOptimizationBE.Data;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Services;

public record DocumentListItemDto(
    Guid Id,
    string Title,
    string Snippet,
    string DocumentTypeName,
    int DocumentTypeId,
    string UploadedBy,
    DateTime UploadedAt,
    int FileSizeKb,
    double? RelevanceScore,
    int? MatchedTokenCount);

public record PaginatedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

public class DocumentQuery
{
    public string? Query { get; set; }
    public int? DocumentTypeId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchService
{
    private readonly AppDbContext _db;

    public SearchService(AppDbContext db) => _db = db;

    public async Task<PaginatedResult<DocumentListItemDto>> SearchAsync(DocumentQuery q, CancellationToken ct)
    {
        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);

        var hasSearch = !string.IsNullOrWhiteSpace(q.Query);

        if (hasSearch)
        {
            return await SearchWithRelevanceAsync(q, page, pageSize, ct);
        }

        return await ListAsync(q, page, pageSize, ct);
    }

    private async Task<PaginatedResult<DocumentListItemDto>> ListAsync(DocumentQuery q, int page, int pageSize, CancellationToken ct)
    {
        var query = _db.Documents
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedBy)
            .AsQueryable();

        query = ApplyFilters(query, q);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(d => d.UploadedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DocumentListItemDto(
                d.Id,
                d.Title,
                d.Content.Length > 160 ? d.Content.Substring(0, 160) + "…" : d.Content,
                d.DocumentType!.Name,
                d.DocumentTypeId,
                d.UploadedBy!.FullName,
                d.UploadedAt,
                d.FileSizeKb,
                null,
                null))
            .ToListAsync(ct);

        return new PaginatedResult<DocumentListItemDto>(items, total, page, pageSize);
    }

    private async Task<PaginatedResult<DocumentListItemDto>> SearchWithRelevanceAsync(DocumentQuery q, int page, int pageSize, CancellationToken ct)
    {
        var queryTokens = Tokenizer.Tokenize(q.Query!).Distinct().ToList();
        if (queryTokens.Count == 0)
        {
            return new PaginatedResult<DocumentListItemDto>(Array.Empty<DocumentListItemDto>(), 0, page, pageSize);
        }

        var tokenMatches = await _db.DocumentTokens
            .AsNoTracking()
            .Where(t => queryTokens.Contains(t.Token))
            .Select(t => new { t.DocumentId, t.Token, t.Field, t.Frequency })
            .ToListAsync(ct);

        if (tokenMatches.Count == 0)
        {
            return new PaginatedResult<DocumentListItemDto>(Array.Empty<DocumentListItemDto>(), 0, page, pageSize);
        }

        var scored = tokenMatches
            .GroupBy(m => m.DocumentId)
            .Select(g => new
            {
                DocumentId = g.Key,
                MatchedTokens = g.Select(x => x.Token).Distinct().Count(),
                TitleHits = g.Where(x => x.Field == TokenField.Title).Sum(x => x.Frequency),
                ContentHits = g.Where(x => x.Field == TokenField.Content).Sum(x => x.Frequency)
            })
            .Select(s => new
            {
                s.DocumentId,
                s.MatchedTokens,
                Score = s.MatchedTokens * 10.0 + s.TitleHits * 5.0 + s.ContentHits * 1.0
            })
            .OrderByDescending(s => s.Score)
            .ToList();

        var docFilter = _db.Documents
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedBy)
            .AsQueryable();

        docFilter = ApplyFilters(docFilter, q);

        var matchedIds = scored.Select(s => s.DocumentId).ToList();
        var docs = await docFilter
            .Where(d => matchedIds.Contains(d.Id))
            .ToListAsync(ct);

        var docsById = docs.ToDictionary(d => d.Id);

        var ordered = scored
            .Where(s => docsById.ContainsKey(s.DocumentId))
            .ToList();

        var total = ordered.Count;

        var pageItems = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s =>
            {
                var d = docsById[s.DocumentId];
                return new DocumentListItemDto(
                    d.Id,
                    d.Title,
                    BuildSnippet(d.Content, queryTokens),
                    d.DocumentType!.Name,
                    d.DocumentTypeId,
                    d.UploadedBy!.FullName,
                    d.UploadedAt,
                    d.FileSizeKb,
                    Math.Round(s.Score, 2),
                    s.MatchedTokens);
            })
            .ToList();

        return new PaginatedResult<DocumentListItemDto>(pageItems, total, page, pageSize);
    }

    private static IQueryable<Document> ApplyFilters(IQueryable<Document> query, DocumentQuery q)
    {
        if (q.DocumentTypeId.HasValue)
            query = query.Where(d => d.DocumentTypeId == q.DocumentTypeId.Value);
        if (q.FromDate.HasValue)
            query = query.Where(d => d.UploadedAt >= q.FromDate.Value);
        if (q.ToDate.HasValue)
            query = query.Where(d => d.UploadedAt <= q.ToDate.Value);
        return query;
    }

    private static string BuildSnippet(string content, IReadOnlyList<string> queryTokens)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;

        var normalized = TurkishNormalizer.Normalize(content);
        var firstHit = -1;
        foreach (var token in queryTokens)
        {
            var idx = normalized.IndexOf(token, StringComparison.Ordinal);
            if (idx >= 0 && (firstHit < 0 || idx < firstHit))
                firstHit = idx;
        }

        const int snippetLen = 160;
        if (firstHit < 0)
            return content.Length > snippetLen ? content[..snippetLen] + "…" : content;

        var start = Math.Max(0, firstHit - 40);
        var end = Math.Min(content.Length, start + snippetLen);
        var prefix = start > 0 ? "…" : string.Empty;
        var suffix = end < content.Length ? "…" : string.Empty;
        return prefix + content.Substring(start, end - start) + suffix;
    }
}
