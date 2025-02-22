using System.Diagnostics;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook.Application.Commands.SendNewsletter;

public abstract record SendNewsletter
{
    protected readonly string Term;
    protected readonly IUserRepository Repository;

    protected SendNewsletter(string term, IUserRepository repository)
    {
        Term = term;
        Repository = repository;
    }

    public virtual string ReplaceWildcard()
        => Term.Replace(Wildcard(), string.Empty);

    public abstract Task<List<User>> GetAllNotifiableAsync(string term);

    public abstract string Wildcard();
}

public record SendNewsletterDebug
    : SendNewsletter
{
    private readonly IAdminConfiguration _adminConfiguration;

    public SendNewsletterDebug(string term, IUserRepository repository, IAdminConfiguration adminConfiguration)
        : base(term, repository)
    {
        _adminConfiguration = adminConfiguration;
    }

    public override Task<List<User>> GetAllNotifiableAsync(string term)
        => Task.FromResult(new List<User> { new(_adminConfiguration.AdminChatId, "Rogim Nazario", "pt") });

    public override string Wildcard() => SendNewsletterType.Debug.ToString();

    public override string ReplaceWildcard()
        => Term.Replace(Wildcard(), string.Empty)
            .Replace(SendNewsletterType.NonVIP.ToString(), "")
            .Replace(SendNewsletterType.VIP.ToString(), "")
            .Replace(SendNewsletterType.General.ToString(), "");
}

public record SendNewsletterVip
    : SendNewsletter
{
    public SendNewsletterVip(string term, IUserRepository repository)
        : base(term, repository) { }

    public override Task<List<User>> GetAllNotifiableAsync(string term)
        => Task.FromResult(Repository.GetAllReadOnly().Where(user => user.vipinformation != "" && user.vipinformation != null).ToArray().Where(user => user.IsVipValid()).ToList());

    public override string Wildcard() => SendNewsletterType.VIP.ToString();
}

public record SendNewsletterNonVip
    : SendNewsletter
{
    public SendNewsletterNonVip(string term, IUserRepository repository)
        : base(term, repository) { }

    public override Task<List<User>> GetAllNotifiableAsync(string term)
        => Task.FromResult(Repository.GetAllReadOnly().Where(user => (user.vipinformation == "" || user.vipinformation == null) && !user.unsubscribe).ToArray().Where(user => user.IsVipValid()).ToList());

    public override string Wildcard() => SendNewsletterType.NonVIP.ToString();
}

public record SendNewsletterGeneral
    : SendNewsletter
{
    public SendNewsletterGeneral(string term, IUserRepository repository)
        : base(term, repository) { }

    public override Task<List<User>> GetAllNotifiableAsync(string term)
        => Task.FromResult(Repository.GetAllReadOnly().Where(user => !user.unsubscribe).ToList());

    public override string Wildcard() => SendNewsletterType.General.ToString();
}

public class SendNewsletterFactory
{
    public static SendNewsletter Create(string term, IUserRepository repository, IAdminConfiguration adminConfiguration)
        => term switch
        {
            var x when x.StartsWith(SendNewsletterType.Debug.ToString()) || Debugger.IsAttached => new SendNewsletterDebug(term, repository, adminConfiguration),
            var x when x.StartsWith(SendNewsletterType.VIP.ToString()) => new SendNewsletterVip(term, repository),
            var x when x.StartsWith(SendNewsletterType.NonVIP.ToString()) => new SendNewsletterNonVip(term, repository),
            var x when x.StartsWith(SendNewsletterType.General.ToString()) => new SendNewsletterGeneral(term, repository),
            _ => throw new NotImplementedException()
        };
}

public class SendNewsletterType
{
    private readonly string _type;

    private SendNewsletterType(string type) => _type = type;

    public static SendNewsletterType Debug => new("/newsletterdebug");
    public static SendNewsletterType VIP => new("/newslettervip");
    public static SendNewsletterType NonVIP => new("/newsletternonvip");
    public static SendNewsletterType General => new("/newsletter");

    public override string ToString() => _type;
}