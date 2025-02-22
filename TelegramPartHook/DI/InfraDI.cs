using Microsoft.Extensions.DependencyInjection;
using System;
using TelegramPartHook.Application.HealthChecks;
using TelegramPartHook.Domain.Constants;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TelegramPartHook.Domain.SeedWork;
using TelegramPartHook.Application.Commands;
using System.Linq;
using System.Reflection;
using MongoDB.Driver;

namespace TelegramPartHook.DI
{
    public static class InfraDI
    {
        public static IServiceCollection AddInfra(this IServiceCollection services)
        {
            services.AddHttpClient();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies()));

            services.AddSignalR();
            services.AddMongoDb();

            services.AddHealthChecks().AddCheck<LoginCheck>("LoginCheck");

            return services.AddMediatorRequests();
        }

        public static IServiceCollection AddInfraCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("DevelopPolicy", policy =>
                {
                    policy
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins("http://localhost:3000",
                                 "https://localhost:3000")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .Build();
                });

                options.AddPolicy("ProdPolicy", policy =>
                {
                    policy
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins("https://partituravip.netlify.app", "http://partituravip.com.br", "https://partituravip.com.br")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .Build();
                });
            });
            return services;
        }

        public static IServiceCollection AddBotAuthentication(this IServiceCollection services, IAdminConfiguration adminConfiguration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new()
                    {
                        RequireExpirationTime = true,
                        ValidIssuer = adminConfiguration.Issuer,
                        ValidateIssuer = true,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(adminConfiguration.ISK))
                    };
                });

            return services;
        }

        public static void AddBotSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(p =>
            {
                p.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Description = "Busca partituras de samba, pagode e choro pela internet.",
                    Title = "Buscador de Partituras API",
                    Contact = new Microsoft.OpenApi.Models.OpenApiContact
                    {
                        Email = "rrnazario@gmail.com",
                        Name = "Rogim Nazario",
                        Url = new Uri("https://t.me/rrnazario")
                    }
                });
            });
        }

        private static IServiceCollection AddMediatorRequests(this IServiceCollection services)
        {
            var botRequestType = typeof(IBotRequest);
            var types = Assembly.GetAssembly(typeof(UnsubscribeCommand))!
                .GetTypes()
                .Where(type => botRequestType.IsAssignableFrom(type) && !type.IsAbstract);

            foreach (var refType in types)
            {
                services.AddScoped(botRequestType, refType);
            }

            return services;
        }

        private static void AddMongoDb(this IServiceCollection services)
        {
            services.AddSingleton<IMongoClient>(sp =>
            {
                var adminConfiguration = sp.GetRequiredService<IAdminConfiguration>();
                
                return new MongoClient(adminConfiguration.MongoConnectionString);
            });
            services.AddScoped(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("partitura-bot");
            });
        }
    }
}
