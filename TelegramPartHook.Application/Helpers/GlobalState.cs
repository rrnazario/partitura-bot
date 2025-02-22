using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Infrastructure.Helpers;
using TelegramPartHook.Infrastructure.Persistence;

namespace TelegramPartHook.Application.Helpers
{
    public interface IGlobalState
    {
        IReadOnlyCollection<string> UserReceiving { get; }
        Config[] ConfigNew { get; }
        bool IsDebug { get; }

        bool MarkGlobalStopSending(string id);
        void MarkGlobalSending(string id);
        void MarkGlobalSearching(string id, string search);
        void MarkGlobalStopSearching(string id, string search);
        bool IsDuplicateSearch(string id, string search);
        void RefreshConfig();
        void CheckCleanFolders();
    }

    public class GlobalState : IGlobalState
    {
        private readonly Lock _locker = new();

        private DateTime _nextCleanDate = DateTime.Now;

        private readonly List<string> _userReceiving = [];
        private readonly Dictionary<string, List<string>> _userSearching = new();
        public IReadOnlyCollection<string> UserReceiving => _userReceiving.AsReadOnly();

        private Config[]? _configNew;
        public Config[] ConfigNew
        {
            get
            {
                if (_configNew is null)
                    RefreshConfig();

                return _configNew!;
            }
        }

        public bool IsDebug => Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

        private readonly IServiceScopeFactory _scopeFactory;

        public GlobalState(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void MarkGlobalSending(string id)
        {
            lock (_locker)
            {
                _userReceiving.Add(id);
            }
        }
        public bool MarkGlobalStopSending(string id)
        {
            var result = false;

            lock (_locker)
            {
                while (_userReceiving.Remove(id)) result = true;
            }

            return result;
        }

        public void RefreshConfig()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BotContext>();

            _configNew = context.Set<Config>().AsNoTracking().ToArray();
        }

        public void CheckCleanFolders()
        {
            var minutesToClean = ConfigNew.First(c => c.Name == Config.MinutesToClean).GetValue<int>();
            
            if (DateTime.Now.Subtract(_nextCleanDate).TotalMinutes > minutesToClean)
            {
                //Get all folders that match with regex.
                var regex = new Regex(@"[\d]{4,}");
                var folders = Directory.GetDirectories(Directory.GetCurrentDirectory(), "*", SearchOption.TopDirectoryOnly)
                              .Select(s => new FileInfo(s))
                              .Where(w => regex.IsMatch(w.Name))
                              .Concat([new FileInfo(Path.GetTempPath())])
                              .ToList();

                if (folders.Any())
                {
                    using var scope = _scopeFactory.CreateScope();
                    var systemHelper = scope.ServiceProvider.GetRequiredService<ISystemHelper>();

                    folders.ForEach(f => systemHelper.DeleteFolder(f.FullName));
                }

                _nextCleanDate = DateTime.Now;
            }
        }

        public void MarkGlobalSearching(string id, string search)
        {
            lock (_locker)
            {
                if (!_userSearching.TryGetValue(id, out var content))
                {
                    content = new();
                }

                content.Add(search);

                _userSearching[id] = content;
            }
        }

        public void MarkGlobalStopSearching(string id, string search)
        {
            lock (_locker)
            {
                if (_userSearching.TryGetValue(id, out var content))
                {
                    content.Remove(search);
                    if (content.Count != 0)
                        _userSearching[id] = content;
                    else
                        _userSearching.Remove(id);
                }
                else
                    _userSearching.Remove(id);
            }
        }

        public bool IsDuplicateSearch(string id, string search)
        {
            lock (_locker)
            {
                return _userSearching.TryGetValue(id, out var content) && content.Contains(search);
            }
        }
    }

}
