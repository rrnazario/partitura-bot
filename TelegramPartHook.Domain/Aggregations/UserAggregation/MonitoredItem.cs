using System.Globalization;

namespace TelegramPartHook.Domain.Aggregations.UserAggregation
{
    public record MonitoredItem
    {
        public string Term { get; set; } = string.Empty;
        public DateTime SearchedDate { get; set; }
        public bool IsValid = true;
        public MonitoredItem(string linha)
        {
            try
            {
                var array = linha.Split('|');

                Term = array.First();
                SearchedDate = DateTime.Parse(array.Last(), new CultureInfo("pt-BR"));
            }
            catch (Exception)
            {
                IsValid = false;
            }
        }

        public MonitoredItem(string term, DateTime searchedDate)
        {
            Term = term;
            SearchedDate = searchedDate;
        }
        
        public override string ToString() => $"{Term}|{SearchedDate:dd/MM/yyyy}";
        public string Format() => $"{Term} ({SearchedDate:dd/MM/yyyy})";
    }
}
