using System.ComponentModel;
using System.Threading.Tasks;

namespace NextGen.src.UI.ViewModels
{
    class LoadingViewModel : INotifyPropertyChanged
    {
        private string _statusMessage = "Инициализация...";
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task InitializeApplicationAsync()
        {
            StatusMessage = "Подключение к базе данных...";
            await Task.Delay(1000); // Имитация длительной операции
            // Здесь код подключения к БД

            StatusMessage = "Загрузка интерфейса...";
            await Task.Delay(1000); // Имитация загрузки интерфейса
            // Здесь код загрузки интерфейса

            StatusMessage = "Завершение настройки...";
            await Task.Delay(1000); // Завершающие шаги
            // Здесь код завершающих настроек

            StatusMessage = "Готово!";
        }
    }
}
