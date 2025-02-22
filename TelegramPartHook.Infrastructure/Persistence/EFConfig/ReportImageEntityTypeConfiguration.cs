using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;

namespace TelegramPartHook.Infrastructure.Persistence.EFConfig;

public class ReportImageEntityTypeConfiguration
    : IEntityTypeConfiguration<ReportImage>
{
    public void Configure(EntityTypeBuilder<ReportImage> builder)
    {
        builder.ToTable("report_images", BotContext.DefaultSchema);

        builder.HasKey(x => x.id);

        builder.Property(p => p.id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Terms)
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<string[]>(v));

        builder.Property(p => p.Url)
            .IsRequired();

        builder.HasIndex(p => p.Url);

        builder.Property(p => p.IsActive)
            .HasDefaultValue(false);
    }
}