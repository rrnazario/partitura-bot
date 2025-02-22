using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace TelegramPartHook.Infrastructure.Persistence
{
    public class BotContext : DbContext
    {
        public const string DefaultSchema = "public";

        public BotContext() { }

        public BotContext(DbContextOptions<BotContext> op) : base(op) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}