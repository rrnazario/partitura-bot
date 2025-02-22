using MediatR;

namespace TelegramPartHook.Domain.SeedWork
{
    public interface IBaseRequest { }
    
    /// <summary>
    /// Implement this interface so that the bot request will be automatically added to DI.
    /// </summary>
    public interface IBotRequest : IBaseRequest, IRequest
    {
        string Prefix { get; }
        bool Match(string term);
    }
    
    public interface IBotRefreshableRequest : IBaseRequest
    {
        void Rehydrate(string term);

        int GetState();
    }
}
