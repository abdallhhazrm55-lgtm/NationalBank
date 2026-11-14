using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Data.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.FullName).IsRequired().HasMaxLength(100);
        builder.Property(c => c.NationalId).IsRequired().HasMaxLength(14);
        builder.Property(c => c.DateOfBirth).HasColumnType("date");
        builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Address).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CustomerType).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.NationalId).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();

        // Have (customer side): removing a customer removes their ownership links.
        builder.HasMany(c => c.CustomerAccounts)
               .WithOne(ca => ca.Customer)
               .HasForeignKey(ca => ca.CustomerId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
