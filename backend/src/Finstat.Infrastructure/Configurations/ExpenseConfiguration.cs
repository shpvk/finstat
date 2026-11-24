using Finstat.Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Finstat.Infrastructure.Configurations;

public class ExpenseConfiguration : IEntityTypeConfiguration<ExpenseEntity>
{
    public void Configure(EntityTypeBuilder<ExpenseEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Amount).HasColumnType("numeric(12,2)");
        builder.Property(e => e.Comment).HasMaxLength(ExpenseEntity.MaxCommentLength);
        builder.HasOne<CategoryEntity>().WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.Date);
    }
}
