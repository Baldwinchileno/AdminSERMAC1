using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AdminSERMAC.Core.Interfaces;
using AdminSERMAC.Core.Infrastructure;
using AdminSERMAC.Services;

namespace AdminSERMAC.Core.Configuration
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            // Logging
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            // Unit of Work y Repositorios
            services.AddScoped<IUnitOfWork>(provider =>
                new UnitOfWork(connectionString, provider.GetRequiredService<ILogger<UnitOfWork>>()));

            // Servicios
            services.AddScoped<IClienteService, ClienteService>();
            services.AddScoped<SQLiteService>();

            // Servicios adicionales
            services.AddSingleton(new ConfigurationService(connectionString));
            services.AddScoped<NotificationService>();
            services.AddScoped<FileDataManager>();

            return services;
        }
    }
}
