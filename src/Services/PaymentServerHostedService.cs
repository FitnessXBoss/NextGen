using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
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
                    webBuilder.UseKestrel();
                    webBuilder.UseUrls("http://localhost:5220");
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Starting web host...");
            await _webHost.StartAsync(cancellationToken);
            Debug.WriteLine("Web host started on http://localhost:5220");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            Debug.WriteLine("Stopping web host...");
            await _webHost.StopAsync(cancellationToken);
            Debug.WriteLine("Web host stopped.");
            _webHost.Dispose();
        }
    }
}
