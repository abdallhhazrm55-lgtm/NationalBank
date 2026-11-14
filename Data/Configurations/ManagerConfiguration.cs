using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NationalBank.Models;

namespace NationalBank.Data.Configurations;

public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FullName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Email).IsRequired().HasMaxLength(150);
        builder.Property(m => m.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(m => m.HireDate).HasColumnType("date");
        builder.HasIndex(m => m.Email).IsUnique();

        // ---- Seed data ----
        builder.HasData(
            new Manager { Id = 1, FullName = "Khaled Mansour", Email = "khaled.mansour@nationalbank.com", PhoneNumber = "01001112233", HireDate = new DateTime(2015, 3, 1) },
            new Manager { Id = 2, FullName = "Dina Farouk",    Email = "dina.farouk@nationalbank.com",    PhoneNumber = "01102223344", HireDate = new DateTime(2018, 7, 15) },
            new Manager { Id = 3, FullName = "Tamer Saad",     Email = "tamer.saad@nationalbank.com",     PhoneNumber = "01203334455", HireDate = new DateTime(2020, 1, 10) });
    }
}
