using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NextGen.src.Services.Api;
using NextGen.src.Services.Document;
using NextGen.src.UI.ViewModels;
using System.Diagnostics;

namespace NextGen.src.Services
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            // Регистрация зависимостей
            services.AddScoped<OrganizationService>();
            services.AddScoped<CarService>();
            services.AddScoped<DocumentGenerator>();
            services.AddScoped<TemplateService>();
            services.AddSingleton<UserSessionService>(UserSessionService.Instance);
            services.AddSingleton<PaymentProcessor>(PaymentProcessor.Instance);

            // Регистрация ViewModel
            services.AddScoped<SalesContractViewModel>();

            // Регистрация фабрики ViewModel
            services.AddScoped<Func<int, SalesContractViewModel>>(serviceProvider => carId =>
            {
                var viewModel = serviceProvider.GetRequiredService<SalesContractViewModel>();
                viewModel.Initialize(carId);
                return viewModel;
            });

            // Регистрация сервиса обновления статуса платежа
            services.AddScoped<IPaymentStatusService, PaymentStatusService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            Debug.WriteLine("Application started and configured.");
        }
    }
}
