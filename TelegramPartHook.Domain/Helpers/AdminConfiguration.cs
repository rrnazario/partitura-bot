using Light.GuardClauses;
using Microsoft.Extensions.Configuration;
using TelegramPartHook.Domain.Aggregations.UserAggregation;

namespace TelegramPartHook.Domain.Constants;

public interface IAdminConfiguration
{
    Dictionary<string, string> Users { get; }
    string ISK { get; }
    string Issuer { get; }
    string TelegramBotToken { get; }
    string AdminChatId { get; }
    string DropboxToken { get; }
    string WebScrapingToken { get; }
    string ConnectionString { get; }
    string MongoConnectionString { get; }
    int MaxFreeSheetsOnRepertoire { get; }

    bool IsUserAdmin(User user);
}

public class AdminConfiguration
    : IAdminConfiguration
{
    private readonly IConfiguration _configuration;

    public AdminConfiguration(IConfiguration configuration)
    {
        _configuration = configuration.MustNotBeNull();
    }

    public Dictionary<string, string> Users
        => _configuration.GetSection(nameof(Users))?.Value?
            .Split(",")
            .ToDictionary(k => k.Split("|").First(), e => e.Split("|").Last()) ?? new();

    public string ISK => _configuration.GetSection(nameof(ISK))?.Value;

    public string Issuer => _configuration.GetSection(nameof(Issuer))?.Value;

    public string TelegramBotToken => _configuration.GetSection(nameof(TelegramBotToken))?.Value;

    public string AdminChatId => _configuration.GetSection(nameof(AdminChatId))?.Value;

    public string DropboxToken => _configuration.GetSection(nameof(DropboxToken))?.Value;

    public string WebScrapingToken => _configuration.GetSection(nameof(WebScrapingToken))?.Value;

    public string ConnectionString => _configuration.GetSection(nameof(ConnectionString))?.Value;

    public string MongoConnectionString => _configuration.GetSection(nameof(MongoConnectionString))?.Value;

    public int MaxFreeSheetsOnRepertoire =>
        int.Parse(_configuration.GetSection(nameof(MaxFreeSheetsOnRepertoire))?.Value ?? "5");

    public bool IsUserAdmin(User user) => user.telegramid == AdminChatId;
}