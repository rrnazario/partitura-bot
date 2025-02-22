using System;
using System.Linq;
using FluentAssertions;
using Light.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Domain.Aggregations.ConfigAggregation;
using TelegramPartHook.Infrastructure.Persistence;
using TelegramPartHook.Tests.Core.Fixtures;
using Xunit;

namespace TelegramPartHook.ComponentTests;

public class ConfigTests
    : IClassFixture<CoreDependencyInjectionFixture>
{
    private readonly ServiceProvider _serviceProvider;

    public ConfigTests(CoreDependencyInjectionFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact]
    public void ConfigTest()
    {
        var tomorrow = DateTime.UtcNow.AddDays(1);

        var ctx = _serviceProvider.GetRequiredService<BotContext>();
        var result = ctx.Set<Config>()
            .First(c => c.Name == ConfigDateTimeName.NextDateToMonitorRun.ToString()).GetDateTimeValue();

        result.Should().MustNotBeNull();
        result.Should().BeCloseTo(tomorrow, TimeSpan.FromSeconds(2));
    }
}