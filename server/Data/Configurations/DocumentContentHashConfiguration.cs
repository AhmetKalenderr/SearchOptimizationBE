using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Data.Configurations;

public class DocumentContentHashConfiguration : IEntityTypeConfiguration<DocumentContentHash>
{
    public void Configure(EntityTypeBuilder<DocumentContentHash> builder)
    {
        builder.ToTable("DocumentContentHashes");
        builder.HasKey(h => h.DocumentId);

        builder.Property(h => h.ContentSha256).IsRequired().HasColumnType("binary(32)");
        builder.Property(h => h.NormalizedTitle).IsRequired().HasMaxLength(500);

        builder.HasOne(h => h.Document)
            .WithOne()
            .HasForeignKey<DocumentContentHash>(h => h.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.ContentSha256)
            .HasDatabaseName("IX_DocumentContentHashes_ContentSha256");

        builder.HasIndex(h => h.NormalizedTitle)
            .HasDatabaseName("IX_DocumentContentHashes_NormalizedTitle");
    }
}
