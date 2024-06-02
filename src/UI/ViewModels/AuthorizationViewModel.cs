using System.Windows;
using System.Windows.Input;
using NextGen.src.Services;
using NextGen.src.Services.Security;
using NextGen.src.UI.Helpers;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.Views;
using static NextGen.src.Services.Security.AuthService;
using System.Windows.Controls;

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
            var (user, errorMessage) = authService.AuthenticateUser(Username, Password);
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
                ShowErrorDialog(errorMessage ?? "Неверное имя пользователя или пароль!");
            }
        }

        private void ShowErrorDialog(string message)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                // Этот код отображает диалоговое окно с ошибкой
                await DialogHost.Show(new TextBlock
                {
                    Text = message,
                    FontSize = 16,
                    Padding = new Thickness(20)
                }, "RootDialog");
            });
        }
    }
}
