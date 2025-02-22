using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;

namespace TelegramPartHook.Infrastructure.Persistence.EFConfig;

internal class ConfigNewEntityTypeConfiguration
    : IEntityTypeConfiguration<Config>
{
    public void Configure(EntityTypeBuilder<Config> builder)
    {
        builder.ToTable("config", BotContext.DefaultSchema)
            .HasKey(t => t.id);

        builder.Property(p => p.id).ValueGeneratedOnAdd();
        builder.Property(p => p.Name).IsRequired();
        builder.Property(p => p.TypeName).IsRequired();
        builder.Property(p => p.Value)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.HasIndex(i => i.Name);
    }
}