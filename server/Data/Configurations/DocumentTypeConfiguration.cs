using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Data.Configurations;

public class DocumentTypeConfiguration : IEntityTypeConfiguration<DocumentType>
{
    public void Configure(EntityTypeBuilder<DocumentType> builder)
    {
        builder.ToTable("DocumentTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);

        builder.HasData(
            new DocumentType { Id = 1, Name = "Sözleşme" },
            new DocumentType { Id = 2, Name = "Teklif" },
            new DocumentType { Id = 3, Name = "Fatura" },
            new DocumentType { Id = 4, Name = "Diğer" }
        );
    }
}
