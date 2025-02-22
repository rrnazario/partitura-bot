using FluentAssertions;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Tests.Core.Helpers;

namespace TelegramPartHook.Domain.UnitTests.Aggregations;

public class SearchTests
{

    private static readonly User userStub = new User(TestHelper.AdminId, "Rogim Nazario", "pt-br", false);

    [Fact]
    public void InvalidParams_CreatingSearch_Throws()
    {
        //Act / Assert
        var searchActionEmpty = () => new Search("", userStub);
        var searchActionSmall = () => new Search("s", userStub);
        var searchActionInvalidUser = () => new Search("valid term", null!);

        searchActionEmpty.Should().Throw<TooSmallSearchException>();
        searchActionSmall.Should().Throw<TooSmallSearchException>();
        searchActionInvalidUser.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenCreatingSearchWithSmallTerm_ThenThrows()
    {
        //Arrange / Act
        Action searchCreation = () => new Search("", null);

        //Assert
        searchCreation.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenCreatingSearchWithNullUser_ThenThrows()
    {
        //Arrange / Act
        Action searchCreation = () => new Search("search", null);

        //Assert
        searchCreation.Should().Throw<ArgumentNullException>();
    }
}