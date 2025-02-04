using Microsoft.Extensions.DependencyInjection;
using Service.CryptographCredentials;
using ServiceExternals;
using ServiceExternals.Interfaces;

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
