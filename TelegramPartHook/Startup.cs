using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelegramPartHook.Application.Hubs;
using TelegramPartHook.DI;
using TelegramPartHook.Domain.Constants;

namespace TelegramPartHook
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddInfraCors();
            services.AddBotSwagger();

            services.AddSingleton(Configuration);
            services.AddMemoryCache();

            var adminConfiguration = new AdminConfiguration(Configuration);
            services.AddSingleton<IAdminConfiguration>(_ => adminConfiguration);

            //Customizations
            services
                .AddBotAuthentication(adminConfiguration)
                .AddPersistence(adminConfiguration)
                .AddHelpers()
                .AddFactories()
                .AddSearchers()
                .AddRoutines()
                .AddErrorHandlers()
                .AddInfra();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwagger();

                app.UseSwaggerUI(p =>
                {
                    p.DocumentTitle = "Buscador de Partituras API";
                    p.EnableFilter();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(Debugger.IsAttached ? "DevelopPolicy" : "ProdPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseErrorHandlers();

            app.ApplyMigrationsOnDatabase();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<LoginHub>("api/hubs/loginHub");
                endpoints.MapHealthChecks("api/loginHealthCheck");
            });
        }
    }
}
