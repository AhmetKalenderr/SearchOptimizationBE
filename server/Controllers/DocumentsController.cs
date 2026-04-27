using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SearchOptimizationBE.Data;
using SearchOptimizationBE.Services;

namespace SearchOptimizationBE.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly SearchService _searchService;
    private readonly DuplicateDetectionService _duplicateService;
    private readonly DocumentUploadService _uploadService;

    public DocumentsController(
        AppDbContext db,
        SearchService searchService,
        DuplicateDetectionService duplicateService,
        DocumentUploadService uploadService)
    {
        _db = db;
        _searchService = searchService;
        _duplicateService = duplicateService;
        _uploadService = uploadService;
    }

    [HttpGet]
    public Task<PaginatedResult<DocumentListItemDto>> List(
        [FromQuery] string? q,
        [FromQuery] int? typeId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        return _searchService.SearchAsync(new DocumentQuery
        {
            Query = q,
            DocumentTypeId = typeId,
            FromDate = from,
            ToDate = to,
            Page = page,
            PageSize = pageSize
        }, ct);
    }

    [HttpGet("types")]
    public async Task<IActionResult> Types(CancellationToken ct)
    {
        var types = await _db.DocumentTypes
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);
        return Ok(types);
    }

    public record CreateDocumentTypeRequest(string Name);

    [HttpPost("types")]
    public async Task<IActionResult> CreateType([FromBody] CreateDocumentTypeRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { error = "Tip adı boş olamaz." });

        var name = req.Name.Trim();
        if (name.Length > 100)
            return BadRequest(new { error = "Tip adı en fazla 100 karakter olabilir." });

        var normalized = TurkishNormalizer.Normalize(name);
        var existing = await _db.DocumentTypes
            .AsNoTracking()
            .Select(t => new { t.Id, t.Name })
            .ToListAsync(ct);

        var dup = existing.FirstOrDefault(t => TurkishNormalizer.Normalize(t.Name) == normalized);
        if (dup is not null)
            return Conflict(new { error = "Bu isimde bir tip zaten var.", existingId = dup.Id, existingName = dup.Name });

        var newType = new Models.DocumentType { Name = name };
        _db.DocumentTypes.Add(newType);
        await _db.SaveChangesAsync(ct);

        return Ok(new { id = newType.Id, name = newType.Name });
    }

    public record CheckDuplicateRequest(string Title, string Content);

    [HttpPost("check-duplicate")]
    public Task<DuplicateCheckResult> CheckDuplicate([FromBody] CheckDuplicateRequest req, CancellationToken ct)
    {
        return _duplicateService.CheckAsync(req.Title ?? string.Empty, req.Content ?? string.Empty, ct);
    }

    [HttpPost]
    public async Task<IActionResult> Upload([FromBody] CreateDocumentRequest req, CancellationToken ct)
    {
        var result = await _uploadService.UploadAsync(req, ct);
        if (!result.Success)
            return Conflict(result);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var doc = await _db.Documents
            .AsNoTracking()
            .Include(d => d.DocumentType)
            .Include(d => d.UploadedBy)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (doc is null) return NotFound();

        return Ok(new
        {
            doc.Id,
            doc.Title,
            doc.Content,
            DocumentTypeName = doc.DocumentType!.Name,
            UploadedBy = doc.UploadedBy!.FullName,
            doc.UploadedAt,
            doc.FileSizeKb
        });
    }
}
