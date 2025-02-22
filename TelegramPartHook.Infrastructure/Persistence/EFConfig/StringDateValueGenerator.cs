using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Infrastructure.Persistence.EFConfig;

public class StringDateValueGenerator : StringValueGenerator
{
    public override string Next(EntityEntry _)
        => DateTime.UtcNow.ToString(DateConstants.DatabaseFormat);
}