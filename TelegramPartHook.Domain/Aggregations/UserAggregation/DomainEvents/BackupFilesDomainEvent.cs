using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.UserAggregation.DomainEvents;

public record BackupFilesDomainEvent(SheetSearchResult[] Sheets) 
    : IDomainEvent;
