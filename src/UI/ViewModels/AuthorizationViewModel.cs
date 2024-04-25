using System.Windows;
using System.Windows.Input;
using NextGen.src.Services.Security;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views;

namespace NextGen.src.UI.ViewModels
{
    public class AuthorizationViewModel
    {
        private string? _username;
        private string? _password;
        public Action? CloseAction { get; set; }

        public string Username
        {
            get { return _username!; } // использование ! для утверждения, что значение не null
            set { _username = value; }
        }

        public string Password
        {
            get { return _password!; } // аналогично
            set { _password = value; }
        }

        public ICommand LoginCommand { get; private set; }

        public AuthorizationViewModel()
        {
            LoginCommand = new RelayCommand(Login);
        }

        private void Login()
        {
            AuthService authService = new AuthService();
            if (authService.AuthenticateUser(Username, Password))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadingWindow loadingWindow = new LoadingWindow();
                    loadingWindow.Show();
                    CloseAction?.Invoke(); // проверка на null перед вызовом
                });
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль!", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
