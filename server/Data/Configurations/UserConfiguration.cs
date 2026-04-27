using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SearchOptimizationBE.Models;

namespace SearchOptimizationBE.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(200);
        builder.Property(u => u.Department).IsRequired().HasMaxLength(100);

        builder.HasData(
            new User { Id = 1, FullName = "Ayşe Yılmaz", Department = "Hukuk" },
            new User { Id = 2, FullName = "Mehmet Demir", Department = "Satınalma" },
            new User { Id = 3, FullName = "Fatma Kaya", Department = "Finans" },
            new User { Id = 4, FullName = "Can Öztürk", Department = "Satış" },
            new User { Id = 5, FullName = "Zeynep Aydın", Department = "İK" },
            new User { Id = 6, FullName = "Burak Şahin", Department = "Operasyon" },
            new User { Id = 7, FullName = "Selin Çelik", Department = "Pazarlama" },
            new User { Id = 8, FullName = "Emre Aslan", Department = "Bilgi Teknolojileri" }
        );
    }
}
