using System.ComponentModel.DataAnnotations;

namespace TelegramPartHook.Domain.SeedWork
{
    public abstract class Entity
    {
        private readonly List<IDomainEvent> _domainEvents = new();
        
        [Key]
        public int id { get; set; }

        public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.ToList();

        public void RaiseEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
        public void ClearEvents() => _domainEvents.Clear();
    }
}
