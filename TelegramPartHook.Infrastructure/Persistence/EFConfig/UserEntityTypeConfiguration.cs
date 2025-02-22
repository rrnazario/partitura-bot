using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Infrastructure.Persistence.EFConfig
{
    internal class UserEntityTypeConfiguration
        : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("client", BotContext.DefaultSchema);

            builder.HasKey(t => t.id);

            builder.Property(p => p.id)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

            builder.Property(p => p.telegramid)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("telegramid")
                .IsRequired();

            builder.Property(p => p.fullname)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("fullname")
                .IsRequired();

            builder.Property(p => p.culture)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("culture")
                .IsRequired()
                .HasDefaultValue("en");

            builder.Property(p => p.isvip)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("isvip");

            builder.Property(p => p.unsubscribe)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("unsubscribe");

            builder.Property(p => p.searchesok)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("searchesok")
                .HasDefaultValue("");

            builder.Property(p => p.vipinformation)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("vipinformation")
                .HasDefaultValue("");

            builder.Property(p => p.lastsearchdate)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("lastsearchdate");

            builder.Property(p => p.searchesscheduled)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("searchesscheduled")
                .HasDefaultValue("");

            builder.Property(p => p.searchescount)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("searchescount");

            builder.Property(p => p.activetokens)
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasColumnName("activetokens")
                .HasDefaultValue("");

            builder.Property(p => p.Repertoire)
                .HasColumnName("repertoire")
                .HasColumnType("jsonb")
                .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Repertoire>(v));
        }
    }
}
