using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.HasKey(b => b.Code);
        builder.Property(b => b.Code).HasMaxLength(10);
        builder.Property(b => b.Name).IsRequired().HasMaxLength(100);
        builder.Property(b => b.Address).IsRequired().HasMaxLength(200);
        builder.Property(b => b.PhoneNumber).IsRequired().HasMaxLength(20);

        // Manage: Branch 1 --- 1 Manager. FK on Branch (unique index is created automatically).
        builder.HasOne(b => b.Manager)
               .WithOne(m => m.Branch)
               .HasForeignKey<Branch>(b => b.ManagerId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        // Belong: Branch 1 --- M Account. A branch that still has accounts cannot be deleted.
        builder.HasMany(b => b.Accounts)
               .WithOne(a => a.Branch)
               .HasForeignKey(a => a.BranchCode)
               .OnDelete(DeleteBehavior.Restrict);

        // ---- Seed data ----
        builder.HasData(
            new Branch { Code = "CAI-01", Name = "Cairo Main Branch", Address = "12 Tahrir Square, Cairo",    PhoneNumber = "0225550001", ManagerId = 1 },
            new Branch { Code = "ALX-01", Name = "Alexandria Branch", Address = "5 Corniche Road, Alexandria", PhoneNumber = "0335550002", ManagerId = 2 },
            new Branch { Code = "GIZ-01", Name = "Giza Branch",       Address = "30 Pyramids Street, Giza",   PhoneNumber = "0235550003", ManagerId = 3 });
    }
}
