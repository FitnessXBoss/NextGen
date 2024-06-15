using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;

namespace NextGen.src.Services
{
    public class PaymentServerHostedService : IHostedService
    {
        private readonly IHost _webHost;

        public PaymentServerHostedService()
        {
            _webHost = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:5220");
                })
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _webHost.StartAsync(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await _webHost.StopAsync(cancellationToken);
            _webHost.Dispose();
        }
    }
}
