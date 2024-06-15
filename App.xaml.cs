using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NextGen.src.Services;
using NextGen.src.Services.Api;
using NextGen.src.Services.Document;
using NextGen.src.UI.ViewModels;

namespace NextGen
{
    public partial class App : Application
    {
        public static IHost AppHost { get; private set; }

        private Process _nodeProcess;
        private const string NodeProcessPidFile = "node_process.pid";
        private static bool _serverStarted = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CultureInfo culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<PaymentServerHostedService>();

                    services.AddScoped<OrganizationService>();
                    services.AddScoped<CarService>();
                    services.AddScoped<DocumentGenerator>();
                    services.AddScoped<TemplateService>();
                    services.AddSingleton<UserSessionService>(UserSessionService.Instance);
                    services.AddSingleton<PaymentProcessor>(PaymentProcessor.Instance);

                    services.AddScoped<SalesContractViewModel>();

                    services.AddScoped<Func<int, SalesContractViewModel>>(serviceProvider => carId =>
                    {
                        var viewModel = serviceProvider.GetRequiredService<SalesContractViewModel>();
                        viewModel.Initialize(carId);
                        return viewModel;
                    });

                    services.AddScoped<IPaymentStatusService, PaymentStatusService>();
                })
                .Build();

            NextGen.src.Services.ServiceProvider.Configure(AppHost.Services);

            if (!_serverStarted)
            {
                _serverStarted = true;
                StartNodeServer();
            }

            AppHost.Start();
        }

        private void StartNodeServer()
        {
            try
            {
                KillProcessOnPort(3001);

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var batFilePath = Path.Combine(basePath, "src", "NodeServer", "start_server.bat");
                var workingDirectory = Path.Combine(basePath, "src", "NodeServer");

                var startInfo = new ProcessStartInfo
                {
                    FileName = batFilePath,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _nodeProcess = new Process
                {
                    StartInfo = startInfo
                };

                _nodeProcess.OutputDataReceived += (sender, args) => Debug.WriteLine(args.Data);
                _nodeProcess.ErrorDataReceived += (sender, args) => Debug.WriteLine(args.Data);

                _nodeProcess.Start();
                _nodeProcess.BeginOutputReadLine();
                _nodeProcess.BeginErrorReadLine();

                File.WriteAllText(NodeProcessPidFile, _nodeProcess.Id.ToString());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Не удалось запустить сервер Node.js: {ex.Message}");
                MessageBox.Show($"Не удалось запустить сервер Node.js: {ex.Message}");
            }
        }

        private void StopNodeServer()
        {
            if (_nodeProcess != null && !_nodeProcess.HasExited)
            {
                try
                {
                    _nodeProcess.Kill();
                    _nodeProcess.WaitForExit(5000);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to stop Node.js server: {ex.Message}");
                }
                finally
                {
                    _nodeProcess.Dispose();
                }
            }

            if (File.Exists(NodeProcessPidFile))
            {
                try
                {
                    var pid = File.ReadAllText(NodeProcessPidFile);
                    KillProcessById(pid);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to stop Node.js server by PID: {ex.Message}");
                }
                finally
                {
                    File.Delete(NodeProcessPidFile);
                }
            }
        }

        private void KillProcessOnPort(int port)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c netstat -ano | findstr :{port}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
            {
                var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length > 4)
                    {
                        string pid = parts[4];
                        KillProcessById(pid);
                    }
                }
            }
        }

        private void KillProcessById(string pid)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c taskkill /PID {pid} /F",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            StopNodeServer();
            AppHost?.StopAsync().Wait();
            AppHost?.Dispose();
            base.OnExit(e);
        }
    }
}
