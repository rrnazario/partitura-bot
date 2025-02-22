using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TelegramPartHook.Application.Commands.ReportImages;
using TelegramPartHook.Application.DTO;
using TelegramPartHook.Application.Factories;
using TelegramPartHook.Application.Services;
using TelegramPartHook.Domain.Aggregations.ReportImageAggregation;
using TelegramPartHook.Domain.Aggregations.SearchCacheAggregation;
using TelegramPartHook.Domain.Aggregations.UserAggregation;
using TelegramPartHook.Domain.Constants;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Fixtures;
using TelegramPartHook.Tests.Core.Helpers;
using Xunit;

namespace TelegramPartHook.ComponentTests;

public record Info(string Address, string Search);
public class ReportImageTests : IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly BotContext _context;
    private readonly ISearchCacheRepository _searchCacheRepository;
    private readonly Mock<ISearchAccessor> _searchAccessor;
    private readonly Mock<IAdminConfiguration> _adminConfiguration;
    private readonly User _user;

    private readonly Info _info = new("some/address", "some/search");

    public ReportImageTests(CoreDependencyInjectionFixture fixture)
    {
        var serviceProvider = fixture.ServiceProvider;

        _context = serviceProvider.GetRequiredService<BotContext>();
        _searchCacheRepository = serviceProvider.GetRequiredService<ISearchCacheRepository>();

        _searchAccessor = new Mock<ISearchAccessor>();
        _adminConfiguration = new Mock<IAdminConfiguration>();
        _user = TestHelper.GenerateUser(_context);
    }

    [Fact]
    public async Task ReportImage_ShouldSaveProperly()
    {
        //Arrange
        var sheetSearchResult = new SheetSearchResult(_info.Address, Enums.FileSource.Crawler).FillId();
        _searchCacheRepository.Add(new SearchCache(_info.Search, [sheetSearchResult]));
        await _searchCacheRepository.SaveChangesAsync();

        _searchAccessor.Setup(s => s.CurrentSearch())
            .Returns(new Search($"/report search {sheetSearchResult.Id}", _user));

        //Act
        var handler =
            new ReportImageCommandHandler(_searchCacheRepository, _context, new Mock<ITelegramSender>().Object, _searchAccessor.Object);
        await handler.Handle(new ReportImageCommand(), CancellationToken.None);

        //Assert
        var reportImages = _context.Set<ReportImage>().AsNoTracking();
        reportImages.Should().HaveCount(1);

        var image = await reportImages.FirstAsync();
        image.Terms.Should().Contain(_info.Search);
        image.Url.Should().Be(_info.Address);
    }

    [Fact]
    public async Task AcceptReportedImage_ShouldKeepTheInformation()
    {
        //Arrange
        var reportImage = new ReportImage(_info.Address, _info.Search);
        await _context.Set<ReportImage>().AddAsync(reportImage);
        await _context.SaveChangesAsync();

        _adminConfiguration.Setup(a => a.IsUserAdmin(_user)).Returns(true);

        _searchAccessor.Setup(s => s.CurrentSearch()).Returns(new Search($"/report-accept {reportImage.id}", _user));

        //Act
        var handler =
            new AcceptReportImageCommandHandler(_searchAccessor.Object, _adminConfiguration.Object,
                _context, new Mock<ITelegramSender>().Object);
        await handler.Handle(new AcceptReportImageCommand(), CancellationToken.None);

        //Assert
        var reportImages = _context.Set<ReportImage>().AsNoTracking();
        reportImages.Should().HaveCount(1);
        var image = reportImages.First();
        image.IsActive.Should().BeTrue();
        image.Terms.Should().Contain(_info.Search);
        image.Url.Should().Be(_info.Address);
    }

    [Fact]
    public async Task RejectReportedImage_ShouldDeleteTheInformation()
    {
        //Arrange
        var reportImage = new ReportImage(_info.Address, _info.Search);
        await _context.Set<ReportImage>().AddAsync(reportImage);
        await _context.SaveChangesAsync();

        _adminConfiguration.Setup(a => a.IsUserAdmin(_user)).Returns(true);

        _searchAccessor.Setup(s => s.CurrentSearch()).Returns(new Search($"/report-reject {reportImage.id}", _user));

        //Act
        var handler =
            new RejectReportImageCommandHandler(_searchAccessor.Object, _adminConfiguration.Object,
                _context, new Mock<ITelegramSender>().Object);
        await handler.Handle(new RejectReportImageCommand(), CancellationToken.None);

        //Assert
        var reportImages = _context.Set<ReportImage>().AsNoTracking();
        reportImages.Should().BeEmpty();
    }
}