using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Interfaces.Searches;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Fixtures;
using TelegramPartHook.Tests.Core.Helpers;
using Xunit;

namespace TelegramPartHook.ComponentTests;

public class APIComponentTests : IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly ServiceProvider _serviceProvider;

    public APIComponentTests(CoreDependencyInjectionFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact(DisplayName = "Adds an item on monitoring")]
    public async Task AddItemOnMonitoring()
    {
        //Arrange
        var term = "sambarril";
        var userTelegramId = TestHelper.GenerateUserId();
        var command = TestHelper.GenerateCommandText($"/monitorar {term}", userTelegramId);

        var searchFactory = _serviceProvider.GetRequiredService<ISearchFactory>();
        var requestFactory = _serviceProvider.GetRequiredService<IRequestFactory>();
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var context = _serviceProvider.GetRequiredService<BotContext>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        var adminConfiguration = _serviceProvider.GetRequiredService<IAdminConfiguration>();

        await unitOfWork.ExecuteSqlRawAsync(
            $"UPDATE client SET searchesscheduled = '' WHERE telegramid = '{userTelegramId}'");

        var search = await searchFactory.CreateSearchAsnc(command);
        var mediatorRequest = requestFactory.DefineRequest(search);

        //Act
        await mediator.Send(mediatorRequest);

        //Assert
        var expectedUser = context.Set<User>().FirstOrDefault(user => user.telegramid == userTelegramId);
        expectedUser!.searchesscheduled.Should().Contain(term);
    }

    [InlineData("gamadinho", true)]
    [InlineData("flex samba", false)]
    [Theory(DisplayName = "Perform a full search")]
    public async Task PerformFullSearches(string term, bool hasResult)
    {
        //Arrange
        var context = _serviceProvider.GetRequiredService<BotContext>();

        var newUser = new User(DateTime.Now.ToString("fffffff"), "Perform FullSearches", "en");

        var command = TestHelper.GenerateCommandText(term, newUser.telegramid);

        var searchFactory = _serviceProvider.GetRequiredService<ISearchFactory>();
        var requestFactory = _serviceProvider.GetRequiredService<IRequestFactory>();
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var adminConfiguration = _serviceProvider.GetRequiredService<IAdminConfiguration>();

        //Act
        var search = await searchFactory.CreateSearchAsnc(command);
        var mediatorRequest = requestFactory.DefineRequest(search);
        await mediator.Send(mediatorRequest);

        var updatedUser =
            await context.Set<User>().FirstOrDefaultAsync(user => user.telegramid == newUser.telegramid);

        //Assert
        if (hasResult)
            updatedUser!.searchesok.Should().Contain(term);
        else
            updatedUser!.searchesok.Should().NotContain(term);

    }

    [InlineData("noel rosa", 16)]
    [InlineData("Quando me quiser turma do pagode", 0)]
    [InlineData("Seguindo Viagem", 0)] //no results
    [Theory]
    public async Task GetPdfChoroSheetsFromAllSources(string termo, int resultado)
    {
        var service = _serviceProvider.GetRequiredService<IChoroSearchService>();

        var result = await service.SearchAsync(termo, CancellationToken.None);

        result.Should().HaveCount(resultado);
    }

    [Fact]
    public async Task GetPdfChoroFilesWithNoAllLinksAllowed()
    {
        //Arrange
        var service = _serviceProvider.GetRequiredService<IChoroSearchService>();

        //Act
        var result = await service.SearchAsync("carinhoso", CancellationToken.None); //there are 6 links in total, but 4 links not allowed. It must return 2.

        //Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPdfChoroWithoutResult()
    {
        var service = _serviceProvider.GetRequiredService<IChoroSearchService>();

        var result = await service.SearchAsync(Guid.NewGuid().ToString(), CancellationToken.None); //Search for a term that does not have result.

        result.Should().BeEmpty();
    }
}