using CryptographCredentials.Framework.LogManagement;
using CryptographCredentials.Framework.LogManagement.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Service.CryptographCredentials;

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
