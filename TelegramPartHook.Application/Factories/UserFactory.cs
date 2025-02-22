using TelegramPartHook.Application.Helpers;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Exceptions;

namespace TelegramPartHook.Application.Factories;

public interface IUserFactory
{
    Task<User> CreateUserDynamicallyAsync(dynamic dynamicContent);
    Task<User> PersistUserAsync(User user);
}
public class UserFactory(IUserRepository userRepository, ILogHelper log) : IUserFactory
{
    public async Task<User> CreateUserDynamicallyAsync(dynamic dynamicContent)
    {
        var isCallback = false;
        string firstName;
        string lastName;
        string telegramId;
        string culture;
        try
        {
            firstName = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["from"]["first_name"];
            lastName = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["from"]["last_name"];

            telegramId = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["from"]["id"];
            culture = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["from"]["language_code"];

            //Initialize(_firstName, _lastName, id, culture);
        }
        catch
        {
            try //try get from callback query
            {
                firstName = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["callback_query"]["from"]["first_name"];
                lastName = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["callback_query"]["from"]["last_name"];
                telegramId = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["callback_query"]["from"]["id"];
                culture = (string)dynamicContent["originalDetectIntentRequest"]["payload"]["data"]["callback_query"]["from"]["language_code"];

                isCallback = true;
            }
            catch
            {
                throw new WrongDynamicContentException(null);
            }
        }

        var fullname = $"{firstName} {lastName}";

        var user = new User(telegramId, fullname, culture, isCallback);

        user = await PersistUserAsync(user);

        return user;
    }

    public async Task<User> PersistUserAsync(User user)
    {
        try {
            var existentUser = await userRepository.GetByIdAsync(user.telegramid);

            if (existentUser is null)
            {
                userRepository.Add(user);
            }
            else
            {
                var updated = existentUser.UpdateDefaultInfo(user.fullname, user.culture);

                if (updated)
                    userRepository.Update(existentUser);

                user = existentUser;
            }
            
            await userRepository.SaveChangesAsync();

        } catch {
            await log.SendMessageToAdminAsync($"Not possible to save user: {user}", CancellationToken.None);
        }

        return user;
    }
}