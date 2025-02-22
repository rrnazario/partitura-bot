using TelegramPartHook.Domain.SeedWork;

namespace TelegramPartHook.Application.Commands
{
    public abstract record BaseBotRefreshableRequest<TEnum>        
        : IBotRefreshableRequest
        where TEnum : Enum
    {
        public string Term { get; private set; }

        public int LastMessageId { get; internal set; }

        public TEnum State { get; private set; }

        protected BaseBotRefreshableRequest(TEnum state)
        {
            State = state;
        }

        public int GetState() => Convert.ToInt32(State);

        public void Rehydrate(string term) => Term = term;

        public void SetNextState(TEnum state) => State = state;

        public void SetLastMessageId(int lastMessageId) => LastMessageId = lastMessageId;

        public string ClearTerm(string infoToClean) => Term.Replace(infoToClean, "").Trim();
    }
}
