using System.Globalization;
using System.Text.Json;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Domain.Aggregations.ConfigAggregation;

public class Config : Entity
{
    public Config()
    {
    }

    public string TypeName { get; set; }
    public object Value { get; private set; }
    public string Name { get; private set; }

    public T GetValue<T>()
    {
        return (T)Convert.ChangeType(Value, Type.GetType(TypeName)!);
    }

    public DateTime GetDateTimeValue()
    {
        var value = JsonSerializer.Deserialize<string>(GetValue<string>());

        DateTime.TryParseExact(value, DateConstants.DatabaseFormat,
            new CultureInfo("pt-BR"), DateTimeStyles.None, out var result);

        return result;
    }

    public Config SetDateTimeValue(DateTime dateTimeValue)
    {
        Value = JsonSerializer.Serialize(dateTimeValue.ToString(DateConstants.DatabaseFormat));
        
        return this;
    }

    public static Config Create<T>(T value, string name)
    {
        return new Config
        {
            TypeName = typeof(T).FullName,
            Value = value,
            Name = name
        };
    }

    public static Config CreateDateTime(DateTime value, ConfigDateTimeName name)
    {
        return new Config
        {
            TypeName = typeof(string).FullName,
            Name = name.ToString()
        }.SetDateTimeValue(value);
    }

    public const string MinutesToClean = nameof(MinutesToClean);
}

public enum ConfigDateTimeName
{
    NextDateToMonitorRun,
    NextDateToCacheClear,
    NextDateSearchOnInstagram
}