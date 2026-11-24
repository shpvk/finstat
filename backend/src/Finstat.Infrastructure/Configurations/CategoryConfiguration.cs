using Finstat.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<CategoryEntity>
{
    public static readonly Guid FoodId = new("3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d01");
    public static readonly Guid UtilitiesId = new("3f1c8a52-6b7e-4d0a-9a41-0c2f5e7b1d02");

    public void Configure(EntityTypeBuilder<CategoryEntity> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.Name).HasColumnType("citext").HasMaxLength(CategoryEntity.MaxNameLength).IsRequired();
        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasData(
            new { Id = FoodId, Name = "Їжа", IsArchived = false },
            new { Id = UtilitiesId, Name = "Комунальні послуги", IsArchived = false });
    }
}
