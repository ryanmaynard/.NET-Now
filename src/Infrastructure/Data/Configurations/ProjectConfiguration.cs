using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(p => p.OwnerId)
            .IsRequired();

        builder.Property(p => p.CreatedBy)
            .HasMaxLength(255);

        builder.Property(p => p.UpdatedBy)
            .HasMaxLength(255);

        builder.HasIndex(p => p.OwnerId);
        builder.HasIndex(p => p.Status);
    }
}
