using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;

namespace TelegramPartHook.Infrastructure.Persistence.EFConfig
{
    internal class SearchCacheEntityTypeConfiguration
        : IEntityTypeConfiguration<SearchCache>
    {
        public void Configure(EntityTypeBuilder<SearchCache> builder)
        {
            builder.ToTable("search", BotContext.DefaultSchema);

            builder.HasKey(t => t.id);

            builder.Property(p => p.id).ValueGeneratedOnAdd();
            builder.Property(p => p.AddedDate)
                .ValueGeneratedOnAdd()
                .HasValueGenerator<StringDateValueGenerator>();

            builder.Property(p => p.Term)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("term")
                .IsRequired();

            builder.Property(p => p.Results)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("results")
                .HasColumnType("jsonb");
            
            builder.HasIndex(i => i.Term);
        }
    }
}
