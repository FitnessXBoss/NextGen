using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views;
using System.Windows.Input;
using System.Windows;

namespace NextGen.src.UI.ViewModels
{
    class DashboardViewModel
    {
        public ICommand ChangeUserCommand { get; private set; }

        public DashboardViewModel()
        {
            ChangeUserCommand = new RelayCommand(ChangeUser);
        }

        private void ChangeUser()
        {
            // Создание и открытие окна авторизации
            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();

            // Закрытие текущего окна
            Application.Current.Dispatcher.Invoke(() => {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(DashboardWindow))
                    {
                        window.Close();
                    }
                }
            });
        }

    }
}
