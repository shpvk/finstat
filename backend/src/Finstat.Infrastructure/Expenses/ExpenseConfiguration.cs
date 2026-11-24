using Finstat.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Finstat.Infrastructure.Expenses;

public class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Amount)
            .HasConversion(m => m.Value, v => new Money(v))
            .HasColumnType("numeric(12,2)");
        builder.Property(e => e.Comment).HasMaxLength(Expense.MaxCommentLength);
        builder.HasOne<Category>().WithMany().HasForeignKey(e => e.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(e => e.Date);
    }
}
