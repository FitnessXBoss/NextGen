using System;
using System.ComponentModel;
using System.Threading.Tasks;
using NextGen.src.Services;

namespace NextGen.src.UI.ViewModels
{
    public class LoadingViewModel : INotifyPropertyChanged
    {
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

        // Конструктор без параметров для использования в XAML
        public LoadingViewModel()
        {

        }

        // Конструктор с параметром для программного создания с зависимостью
        public LoadingViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task InitializeApplicationAsync()
        {
            if (_databaseService == null)
            {
                _databaseService = new DatabaseService(); // Ленивая инициализация
            }

            StatusMessage = "Подключение к базе данных...";
            try
            {
                using (var connection = _databaseService.GetConnection())
                {
                    await Task.Delay(1000); // Имитация длительной операции
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка подключения к базе данных: " + ex.Message;
                return;
            }

            StatusMessage = "Загрузка интерфейса...";
            await Task.Delay(1000); // Имитация загрузки интерфейса

            StatusMessage = "Завершение настройки...";
            await Task.Delay(1000); // Завершающие шаги

            StatusMessage = "Готово!";
        }

    }
}
