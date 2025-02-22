using System.Text.Json.Serialization;
using Light.GuardClauses;
using TelegramPartHook.Domain.Aggregations.UserAggregation.DomainEvents;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.UserAggregation
{
    public class User
        : Entity
    {
        public string telegramid { get; }
        public string fullname { get; private set; }
        public string culture { get; private set; } = "en";
        public bool isvip { get; private set; } = false;
        public bool unsubscribe { get; private set; } = false;
        public string searchesok { get; private set; } = "";
        public string searchesscheduled { get; private set; } = "";
        public string vipinformation { get; private set; } = "";
        public int searchescount { get; private set; } = 0;
        public string lastsearchdate { get; private set; } = DateTime.Now.ToString(DateConstants.DatabaseFormat);
        public string activetokens { get; private set; } = "";
        public Repertoire Repertoire { get; private set; }

        [JsonIgnore]
        private Lazy<VipInformation> _vipInformation;

        [JsonIgnore]
        public bool IsCallback = false;

        [JsonIgnore]
        private string _searchFolder;

        [JsonIgnore]
        public string SearchFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_searchFolder))
                {
                    _searchFolder = $"{telegramid}/{DateTime.Now:ddMMyyyyHHmmssfff}";

                    _searchFolder = Path.Combine(Directory.GetCurrentDirectory(), _searchFolder);

                    if (!Directory.Exists(_searchFolder))
                        Directory.CreateDirectory(_searchFolder);
                }

                return _searchFolder;
            }
        }

        private User()
        {
            _vipInformation = new Lazy<VipInformation>(() => new VipInformation(vipinformation, isvip));
            InitializeRepertoire();
        }

        public User(string telegramId, string fullName, string _culture = "pt-br", bool isCallback = false)
            : this()
        {
            telegramid = telegramId.MustNotBeNullOrEmpty();
            fullname = fullName.MustNotBeNullOrEmpty();
            culture = string.IsNullOrEmpty(_culture) ? "pt-br" : _culture;

            IsCallback = isCallback;
        }

        public VipInformation GetRawVipInfo() => _vipInformation.Value;
        public string GetPortalUsername() => GetRawVipInfo().PortalUser;

        public void UpdateScheduledSearch(string newTerm)
        {
            if (string.IsNullOrEmpty(searchesscheduled) ||
                    !searchesscheduled.Contains(newTerm, StringComparison.InvariantCultureIgnoreCase))
            {

                var newItem = new MonitoredItem(newTerm, DateTime.Now);

                searchesscheduled = !string.IsNullOrEmpty(searchesscheduled)
                                             ? string.Join(",", searchesscheduled, newItem.ToString())
                                             : newItem.ToString();
            }
        }

        public void UpdateScheduledSearch(List<string> keptItems)
        {
            searchesscheduled = keptItems.Count != 0 ? string.Join(",", keptItems) : string.Empty;
        }

        public void Unsubscribe()
        {
            if (!unsubscribe)
                unsubscribe = true;
        }

        public User Upgrade(string potalUser, string expireDate)
        {
            isvip = true;
            vipinformation = $"{expireDate}\n{potalUser}";

            return this;
        }

        public void UpdateSearchState(string term, SheetSearchResult[] parts)
        {
            if (parts.Any())
            {
                searchesok = UpdateSearch(searchesok, term);

                RaiseEvent(new BackupFilesDomainEvent(parts));
            }

            searchescount++;
            unsubscribe = false;
            lastsearchdate = DateTime.Now.ToString(DateConstants.DatabaseFormat);
        }

        public MonitoredItem[] GetMonitoredItems()
        {
            //first = search term; last = search date
            return searchesscheduled.Split(',')
                .Select(s => new MonitoredItem(s.Trim()))
                .Where(s => s.IsValid)
                .ToArray();
        }

        public bool UpdateDefaultInfo(string newFullName, string newCulture)
        {
            if (!fullname.Equals(newFullName) || !culture.Equals(newCulture))
            {
                fullname = newFullName;
                culture = newCulture;

                return true;
            }

            return false;
        }

        public void RemoveScheduledSearch(MonitoredItem item)
        {
            var monitored = GetMonitoredItems().ToList();
            if (monitored.Remove(item))
            {
                searchesscheduled = string.Join(",", monitored.Select(s => s.ToString()));
            }
        }

        public string GetVipMessage() => _vipInformation.Value.ToString();

        public bool IsVipValid() => _vipInformation.Value.IsVipValid();

        public override string ToString() => $"{fullname} ({telegramid}) VIP: {_vipInformation.Value.IsVipValid()}";

        public void UpdateActiveTokens(string newToken)
        {
            activetokens ??= "";

            if (activetokens.Contains(newToken)) return;

            var tokens = activetokens.Split('|').ToList();
            if (tokens.Count() >= 2)
            {
                tokens.RemoveAt(0);
            }

            tokens.Add(newToken);

            activetokens = string.Join("|", tokens);
        }

        public bool RemoveToken(string token)
        {
            activetokens ??= "";

            if (!IsTokenValid(token)) return false;

            var tokens = activetokens.Split('|').ToList();

            var removed = tokens.Remove(token);

            activetokens = string.Join("|", tokens);

            return removed;
        }

        public bool IsTokenValid(string token) => !string.IsNullOrEmpty(token) && activetokens.Contains(token);

        public void InitializeRepertoire()
        {
            Repertoire ??= new();
        }

        private string UpdateSearch(string search, string newTerm)
        {
            if (string.IsNullOrEmpty(search))
                search = newTerm;
            else if (!search.Contains(newTerm, StringComparison.InvariantCultureIgnoreCase))
                search = string.Join(",", newTerm ?? "", search);

            return search;
        }
    }
}
