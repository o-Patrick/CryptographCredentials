using CryptographCredentials.IoC;
using CryptographCredentials.Service;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CryptographCredentials.Worker
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
                .ConfigureServices((host, services) => { services.AddIoC(); });
        }

        internal static void Execute(IServiceProvider serviceProvider)
        {
            var service = serviceProvider.GetRequiredService<CryptographCredentialsService>();
            service.ExecuteAsync().Wait();
        }
    }
}
