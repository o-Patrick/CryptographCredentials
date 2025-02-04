using CryptographCredentials.IoC;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Service.CryptographCredentials;

namespace CryptographCredentials
{
    internal class Program
    {
        static void Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            Execute(host.Services);
        }

        internal static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder()
                .ConfigureServices((host, services) =>
                {
                    services.AddIoC();
                });
        }

        internal static void Execute(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<CryptographCredentialsService>();

            service.Execute();
        }
    }
}
