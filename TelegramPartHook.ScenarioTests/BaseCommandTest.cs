using Microsoft.Extensions.DependencyInjection;
using TelegramPartHook.Tests.Core.Fixtures;

namespace TelegramPartHook.ComponentTests;

public abstract class BaseCommandTest(CoreDependencyInjectionFixture fixture)
{
    protected readonly ServiceProvider ServiceProvider = fixture.ServiceProvider;
}