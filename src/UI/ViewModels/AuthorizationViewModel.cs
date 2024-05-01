using System.Windows;
using System.Windows.Input;
using NextGen.src.Services;
using NextGen.src.Services.Security;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views;
using static NextGen.src.Services.Security.AuthService;

namespace NextGen.src.UI.ViewModels
{
    public class AuthorizationViewModel
    {
        private string? _username;
        private string? _password;
        public Action? CloseAction { get; set; }

        public string Username
        {
            get { return _username!; }
            set { _username = value; }
        }

        public string Password
        {
            get { return _password!; }
            set { _password = value; }
        }

        public ICommand LoginCommand { get; private set; }
        public ICommand ToggleThemeCommand { get; private set; }


        public static UserAuthData? CurrentUser { get; private set; }

        public AuthorizationViewModel()
        {
            LoginCommand = new RelayCommand(Login);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
        }

        private void ToggleTheme()
        {
            var themeService = new ThemeService();
            themeService.ToggleTheme();
        }

        private void Login()
        {
            AuthService authService = new AuthService();
            var user = authService.AuthenticateUser(Username, Password);
            if (user != null)
            {
                CurrentUser = user;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    LoadingWindow loadingWindow = new LoadingWindow();
                    loadingWindow.Show();
                    CloseAction?.Invoke();
                });
            }
            else
            {
                MessageBox.Show("Неверное имя пользователя или пароль!", "Ошибка входа", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
