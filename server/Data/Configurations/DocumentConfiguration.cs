using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Data.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title).IsRequired().HasMaxLength(500);
        builder.Property(d => d.Content).IsRequired();
        builder.Property(d => d.UploadedAt).IsRequired();
        builder.Property(d => d.FileSizeKb).IsRequired();

        builder.HasOne(d => d.DocumentType)
            .WithMany()
            .HasForeignKey(d => d.DocumentTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.UploadedBy)
            .WithMany()
            .HasForeignKey(d => d.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.UploadedAt);
        builder.HasIndex(d => d.DocumentTypeId);
    }
}
