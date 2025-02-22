using FluentAssertions;
using Light.GuardClauses.Exceptions;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.Domain.UnitTests.Aggregations;

public class UserTest
{
    [Fact]
    public void InvalidParams_CreatingUser_Throws()
    {
        string fullname = "Rogim",
            culture = "pt-br",
            telegramid = "8989";
            
            
        var userTgIdNull = () => new User("", fullname, culture);
        var userFullNameNull = () => new User(telegramid, "", culture);

        userTgIdNull.Should().Throw<EmptyStringException>();
        userFullNameNull.Should().Throw<EmptyStringException>();
    }

    [Fact]
    public void UpdatingUser_CultureChanges()
    {
        var user = new User(TestHelper.AdminId, "Rogim Nazario", "en");

        var oldCulture = user.culture;
        
        user.UpdateDefaultInfo("Rogim Nazario", "pt-br");

        oldCulture.Should().NotBeSameAs(user.culture);
    }
}