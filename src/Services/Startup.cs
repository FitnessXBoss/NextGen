using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NextGen.src.Services.Api;
using NextGen.src.Services.Document;
using NextGen.src.UI.ViewModels;
using NextGen.src.Models; // Добавьте этот using для доступа к PaymentSettings

namespace NextGen.src.Services
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddScoped<OrganizationService>();
            services.AddScoped<CarService>();
            services.AddScoped<DocumentGenerator>();
            services.AddScoped<TemplateService>();
            services.AddSingleton<UserSessionService>(UserSessionService.Instance);
            services.AddSingleton<PaymentProcessor>(PaymentProcessor.Instance);

            services.AddScoped<SalesContractViewModel>();

            services.AddScoped<IPaymentStatusService, PaymentStatusService>();

            // Регистрация PaymentSettings
            services.AddSingleton<PaymentSettings>();

            // Регистрация фабрики ViewModel
            services.AddScoped<Func<int, SalesContractViewModel>>(serviceProvider => carId =>
            {
                var viewModel = serviceProvider.GetRequiredService<SalesContractViewModel>();
                viewModel.Initialize(carId);
                return viewModel;
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints => endpoints.MapControllers());

            System.Diagnostics.Debug.WriteLine("Application started and configured.");
        }
    }
}
