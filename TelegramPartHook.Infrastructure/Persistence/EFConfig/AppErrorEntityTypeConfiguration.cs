using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;
using TelegramPartHook.Infrastructure.Models;

namespace TelegramPartHook.Infrastructure.Persistence.EFConfig;

internal class AppErrorEntityTypeConfiguration
    : IEntityTypeConfiguration<AppError>
{
    public void Configure(EntityTypeBuilder<AppError> builder)
    {
        builder.ToTable("apperror", BotContext.DefaultSchema);

        builder.HasKey(t => t.id);

        builder.Property(p => p.id)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(p => p.ErrorDate)
            .HasColumnName("error_date")
            .ValueGeneratedOnAdd()
            .HasValueGenerator<StringDateValueGenerator>();

        builder.Property(p => p.Content)
            .HasColumnName("content")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),
                v => JsonConvert.DeserializeObject<Exception>(v));
    }
}