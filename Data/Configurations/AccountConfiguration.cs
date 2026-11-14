using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.HasKey(a => a.AccountNumber);
        builder.Property(a => a.AccountNumber).HasMaxLength(20);
        builder.Property(a => a.CurrentBalance).HasPrecision(18, 2);
        builder.Property(a => a.AccountType).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.OpeningDate).HasColumnType("date");
        builder.Property(a => a.BranchCode).IsRequired().HasMaxLength(10);

        // Branch relationship is configured in BranchConfiguration.

        // Have (account side): removing an account removes its ownership links.
        builder.HasMany(a => a.CustomerAccounts)
               .WithOne(ca => ca.Account)
               .HasForeignKey(ca => ca.AccountNumber)
               .OnDelete(DeleteBehavior.Cascade);

        // Related: Account 1 --- M Transaction. Removing an account removes its transactions.
        builder.HasMany(a => a.Transactions)
               .WithOne(t => t.Account)
               .HasForeignKey(t => t.AccountNumber)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
