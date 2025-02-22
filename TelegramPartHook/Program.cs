using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;

namespace TelegramPartHook
{
    public class Program
    {
        public static void Main(string[] args)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);            
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog((_, configuration) =>
                configuration
#if DEBUG
                .MinimumLevel.Debug()
#else
                .MinimumLevel.Information()
#endif

                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}"))
                .ConfigureWebHostDefaults(_ => _.UseStartup<Startup>())
                .UseDefaultServiceProvider((_, spOptions) => {
                    spOptions.ValidateScopes = true;
                    spOptions.ValidateOnBuild = true;
                });
    }
}
