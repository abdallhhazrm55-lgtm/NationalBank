using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Data.Configurations;

public class CustomerAccountConfiguration : IEntityTypeConfiguration<CustomerAccount>
{
    public void Configure(EntityTypeBuilder<CustomerAccount> builder)
    {
        // Composite key: a customer can be linked to a given account only once.
        builder.HasKey(ca => new { ca.CustomerId, ca.AccountNumber });

        builder.Property(ca => ca.AccountNumber).HasMaxLength(20);
        builder.Property(ca => ca.OwnershipType).HasConversion<string>().HasMaxLength(20);
        builder.Property(ca => ca.AccountStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(ca => ca.OwnershipStartDate).HasColumnType("date");

        // Both relationships are configured on the principal sides
        // (CustomerConfiguration and AccountConfiguration).
    }
}
