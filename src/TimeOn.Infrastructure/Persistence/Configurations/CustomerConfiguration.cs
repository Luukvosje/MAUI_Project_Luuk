using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TimeOn.Domain.Entities;

namespace TimeOn.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(customer => customer.Id);

        builder.Property(customer => customer.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(customer => customer.Address)
            .HasMaxLength(500);

        builder.Property(customer => customer.ContactEmail)
            .HasMaxLength(320);

        builder.Property(customer => customer.IsActive)
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(customer => customer.LastSyncedAtUtc)
            .IsRequired();

        builder.OwnsOne(customer => customer.Location);
    }
}
