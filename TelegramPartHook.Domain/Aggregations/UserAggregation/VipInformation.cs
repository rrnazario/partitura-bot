using System.Globalization;

namespace TelegramPartHook.Domain.Aggregations.UserAggregation
{
    public record VipInformation
    {
        public DateTime ExpirationDate { get; }
        public string PortalUser { get; }
        public bool IsVip { get; }

        public VipInformation(string rawInformation, bool isVip)
        {
            IsVip = isVip;
            rawInformation ??= "";

            var arrayInfo = rawInformation.Split('\n');
            if (arrayInfo.Length >= 1)
            {
                if (DateTime.TryParse(arrayInfo.First().Trim(), new CultureInfo("pt-br"), DateTimeStyles.AssumeUniversal, out var dt))
                {
                    ExpirationDate = dt;
                }

                PortalUser = arrayInfo.Length > 1 ? arrayInfo.Skip(1).First().Trim() : string.Empty;
            }
        }

        public bool IsVipValid() => IsVip && ExpirationDate >= DateTime.UtcNow.AddDays(1).Date;

        public override string ToString()
            => IsVipValid()
            ? $"*Data de expiração:* {ExpirationDate.AddDays(1):dd/MM/yyyy}\n*Login para o portal VIP*: {PortalUser}"
            : string.Empty;
    }
}
