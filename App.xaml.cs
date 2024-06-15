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
        private IHost _host;
        private Process _nodeProcess;
        private const string NodeProcessPidFile = "node_process.pid";
        private static bool _serverStarted = false; // Добавляем флаг для предотвращения двойного запуска

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Установить культуру для всего приложения
            CultureInfo culture = new CultureInfo("ru-RU");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // Установить культуру для всех элементов
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(
                    XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<PaymentServerHostedService>();

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
                })
                .Build();

            NextGen.src.Services.ServiceProvider.Configure(_host.Services);

            if (!_serverStarted) // Проверяем флаг перед запуском сервера
            {
                _serverStarted = true;
                StartNodeServer();
            }

            _host.Start();
        }

        private void StartNodeServer()
        {
            try
            {
                // Убейте процесс, занимающий порт 3001 (или другой порт, который вы используете)
                KillProcessOnPort(3001);

                // Получаем базовую директорию приложения
                var basePath = AppDomain.CurrentDomain.BaseDirectory;

                // Вычисляем относительные пути к batch файлу и его рабочей директории
                var batFilePath = Path.Combine(basePath, "src", "NodeServer", "start_server.bat");
                var workingDirectory = Path.Combine(basePath, "src", "NodeServer");

                Debug.WriteLine($"Запуск сервера Node.js...");
                Debug.WriteLine($"batFilePath: {batFilePath}");
                Debug.WriteLine($"workingDirectory: {workingDirectory}");

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

                // Сохранение PID процесса в файл
                File.WriteAllText(NodeProcessPidFile, _nodeProcess.Id.ToString());

                Debug.WriteLine("Сервер Node.js запущен.");
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
                    _nodeProcess.WaitForExit(5000); // Ждем до 5 секунд для завершения процесса
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

            // Попытка закрыть процесс по сохраненному PID
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
                        Debug.WriteLine($"Terminating process on port {port}: PID {pid}");
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

            Debug.WriteLine($"Process with PID {pid} terminated.");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            StopNodeServer();
            _host?.StopAsync().Wait();
            _host?.Dispose();
            base.OnExit(e);
        }
    }
}
