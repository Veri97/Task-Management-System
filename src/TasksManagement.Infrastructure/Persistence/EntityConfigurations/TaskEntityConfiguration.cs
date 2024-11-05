using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using TasksManagement.Core.Entities;

namespace TasksManagement.Infrastructure.Persistence.EntityConfigurations;

internal sealed class TaskEntityConfiguration : IEntityTypeConfiguration<TaskEntity>
{
    public void Configure(EntityTypeBuilder<TaskEntity> builder)
    {
        builder.ToTable("Tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(t => t.Description)
               .HasMaxLength(4000)
               .IsRequired();

        builder.Property(t => t.Status)
               .HasConversion<string>()
               .HasMaxLength(20)
               .IsRequired();

        builder.Property(t => t.AssignedTo)
               .HasMaxLength(500);

        builder.HasIndex(t => t.Name)
               .IsUnique();
    }
}