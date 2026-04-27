using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Data.Configurations;

public class DocumentTokenConfiguration : IEntityTypeConfiguration<DocumentToken>
{
    public void Configure(EntityTypeBuilder<DocumentToken> builder)
    {
        builder.ToTable("DocumentTokens");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Token).IsRequired().HasMaxLength(64);
        builder.Property(t => t.Field).IsRequired();
        builder.Property(t => t.Frequency).IsRequired();

        builder.HasOne(t => t.Document)
            .WithMany()
            .HasForeignKey(t => t.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(t => new { t.Token, t.DocumentId, t.Field, t.Frequency })
            .HasDatabaseName("IX_DocumentTokens_Token_Lookup");

        builder.HasIndex(t => t.DocumentId)
            .HasDatabaseName("IX_DocumentTokens_DocumentId");
    }
}
