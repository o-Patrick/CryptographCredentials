using CryptographCredentials.Framework.LogManagement;
using CryptographCredentials.Framework.LogManagement.Interfaces;
using CryptographCredentials.Service;
using Microsoft.Extensions.DependencyInjection;

namespace CryptographCredentials.IoC
{
    public static class Bootstrapper
    {
        public static void AddIoC(this IServiceCollection services)
        {
            services.AddScoped<ILogHandler, LogHandler>();
            services.AddScoped<CryptographCredentialsService>();
        }
    }
}
