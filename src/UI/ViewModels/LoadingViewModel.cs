using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using NextGen.src.Services;
using NextGen.src.UI.Views;

namespace NextGen.src.UI.ViewModels
{
    public class LoadingViewModel : INotifyPropertyChanged
    {
        private Window? _currentWindow;
        private string _statusMessage = "Инициализация...";
        private DatabaseService? _databaseService;

        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged(nameof(StatusMessage));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public LoadingViewModel(Window currentWindow)
        {
            _currentWindow = currentWindow;
        }

        public LoadingViewModel()
        {
        }

        public LoadingViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task InitializeApplicationAsync()
        {
            if (_databaseService == null)
            {
                _databaseService = new DatabaseService();
            }

            StatusMessage = "Подключение к базе данных...";
            try
            {
                using (var connection = await _databaseService.GetConnectionAsync())
                {
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка подключения к базе данных: " + ex.Message;
                return;
            }

            StatusMessage = "Загрузка данных пользователя...";
            await LoadUserDataAsync(); // Загрузка данных пользователя

            StatusMessage = "Загрузка интерфейса...";
            await Task.Delay(1000);

            StatusMessage = "Завершение настройки...";
            await Task.Delay(1000);

            StatusMessage = "Готово!";

            OpenDashboardWindow(); // Открытие Dashboard окна
        }

        private async Task LoadUserDataAsync()
        {
            // Предполагаем, что у вас есть метод в UserSessionService для загрузки данных
            await UserSessionService.Instance.LoadAdditionalUserDataAsync();
            var currentUser = UserSessionService.Instance.CurrentUser;
            if (currentUser != null)
            {
                // Загрузка данных пользователя в UserSessionService
            }
            else
            {
                Console.WriteLine("CurrentUser is null after LoadAdditionalUserData");
            }
        }

        private void OpenDashboardWindow()
        {
            Application.Current.Dispatcher.Invoke(() => {
                var dashboardWindow = new DashboardWindow();
                dashboardWindow.Show();
                _currentWindow?.Close(); // Закрытие текущего окна
            });
        }
    }
}
