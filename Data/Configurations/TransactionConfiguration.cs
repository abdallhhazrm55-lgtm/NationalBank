using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.TransactionNumber);
        builder.Property(t => t.TransactionNumber).ValueGeneratedOnAdd();
        builder.Property(t => t.Amount).HasPrecision(18, 2);
        builder.Property(t => t.TransactionType).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Note).HasMaxLength(250);
        builder.Property(t => t.AccountNumber).IsRequired().HasMaxLength(20);

        builder.ToTable(t => t.HasCheckConstraint("CK_Transaction_Amount", "[Amount] > 0"));
    }
}
