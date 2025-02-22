using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramPartHook.Domain.Aggregations.InstagramCacheAggregation;

namespace TelegramPartHook.Infrastructure.Persistence.EFConfig
{
    internal class InstaCacheEntityTypeConfiguration
        : IEntityTypeConfiguration<InstagramCache>
    {
        public void Configure(EntityTypeBuilder<InstagramCache> builder)
        {
            builder.ToTable("instacache", BotContext.DefaultSchema);

            builder.HasKey(t => t.id);

            builder.Property(p => p.id).ValueGeneratedOnAdd();

            builder.Property(p => p.PageName)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("pagename")
                .IsRequired();

            builder.Property(p => p.Items)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("items")
                .HasColumnType("jsonb");

                builder.Property(p => p.GraphqlId)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("graphqlid")
                .IsRequired();

                builder.Property(p => p.EndCursor)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("endcursor")
                .IsRequired();
        }
    }
}
