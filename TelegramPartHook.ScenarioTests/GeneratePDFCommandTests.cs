using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TelegramPartHook.Application.Commands;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Exceptions;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Tests.Core.Fixtures;
using TelegramPartHook.Tests.Core.Helpers;
using Xunit;


namespace TelegramPartHook.ComponentTests;

public class GeneratePDFCommandTests
    : BaseCommandTest, IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly IMediator _mediator;
    private readonly ITelegramSender _sender;
    private readonly ISearchAccessor _accessor;
    private IPdfService? _pdfService;

    public GeneratePDFCommandTests(CoreDependencyInjectionFixture fixture) : base(fixture)
    {
        _mediator = ServiceProvider.GetRequiredService<IMediator>();
        _sender = ServiceProvider.GetRequiredService<ITelegramSender>();
        _accessor = ServiceProvider.GetRequiredService<ISearchAccessor>();
    }

    [InlineData("71715998-fbce-46cb-953e-44aa72012523", 0)]
    [InlineData("sambarril", 1)]
    [Theory]
    public async Task GeneratePDFTest(string term, int times)
    {
        //Arrange
        var commandText = TestHelper.GenerateAdminCommandText($"/pdf {term}");
        var search = await CreateSearchAsync(commandText);

        _accessor.SetCurrentSearch(search);

        var pdfMock = new Mock<IPdfService>();
        _pdfService = pdfMock.Object;

        var req = new GeneratePDFCommand();
        var handler = CreateTarget();

        await handler.Handle(req, CancellationToken.None);

        pdfMock.Verify(service => service.GenerateAsync(It.IsAny<SheetSearchResult[]>(), It.IsAny<string>()),
            Times.Exactly(times));
    }

    [Fact]
    public async Task GeneratePDFTest_WithShortTerm_ShouldThrow()
    {
        //Arrange
        var commandText = TestHelper.GenerateAdminCommandText("/pdf");
        var search = await CreateSearchAsync(commandText);

        _accessor.SetCurrentSearch(search);

        var pdfMock = new Mock<IPdfService>();
        _pdfService = pdfMock.Object;

        var req = new GeneratePDFCommand();
        var handler = CreateTarget();

        var act = () => handler.Handle(req, CancellationToken.None).GetAwaiter().GetResult();

        act.Should().Throw<TooSmallSearchException>();
        pdfMock.Verify(pdfService => pdfService.GenerateAsync(It.IsAny<SheetSearchResult[]>(), It.IsAny<string>()),
            Times.Never);
    }

    private async Task<Search> CreateSearchAsync(string commandText)
    {
        var searchFactory = ServiceProvider.GetRequiredService<ISearchFactory>();

        return await searchFactory.CreateSearchAsnc(commandText);
    }

    private GeneratePDFCommandHandler CreateTarget()
        => new(_sender, _mediator, _pdfService!, _accessor);
}