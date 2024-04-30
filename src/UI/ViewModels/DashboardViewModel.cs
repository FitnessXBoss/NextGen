using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using NextGen.src.Services;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views;
using NextGen.src.Services.Security;

namespace NextGen.src.UI.ViewModels
{
    class DashboardViewModel : INotifyPropertyChanged
    {
        public ICommand ChangeUserCommand { get; private set; }
        public ICommand MinimizeCommand { get; private set; }
        public ICommand MaximizeRestoreCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand ToggleThemeCommand { get; private set; }
        public ICommand ToggleDrawerCommand { get; private set; }

        private bool _isDrawerOpen;
        public bool IsDrawerOpen
        {
            get => _isDrawerOpen;
            set
            {
                _isDrawerOpen = value;
                OnPropertyChanged(nameof(IsDrawerOpen));
            }
        }

        private DashboardViewModel()
        {
            ChangeUserCommand = new RelayCommand(ChangeUser);
            MinimizeCommand = new RelayCommand(MinimizeWindow);
            MaximizeRestoreCommand = new RelayCommand(MaximizeRestoreWindow);
            CloseCommand = new RelayCommand(CloseWindow);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ToggleDrawerCommand = new RelayCommand(ToggleDrawer);
        }


        public static async Task<DashboardViewModel> CreateAsync()
        {
            var viewModel = new DashboardViewModel();
            await viewModel.InitializeUserDataAsync();
            return viewModel;
        }

        private void ToggleTheme()
        {
            // Логика для изменения темы
        }

        private void ToggleDrawer()
        {
            IsDrawerOpen = !IsDrawerOpen;
        }

        private void ChangeUser()
        {
            AuthorizationWindow authWindow = new AuthorizationWindow();
            authWindow.Show();
            CloseCurrentWindow();
        }

        private void MinimizeWindow()
        {
            var dashboardWindow = GetDashboardWindow();
            if (dashboardWindow != null)
            {
                dashboardWindow.WindowState = WindowState.Minimized;
            }
        }

        private void MaximizeRestoreWindow()
        {
            var dashboardWindow = GetDashboardWindow();
            if (dashboardWindow != null)
            {
                dashboardWindow.WindowState = dashboardWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
        }

        private void CloseWindow()
        {
            var dashboardWindow = GetDashboardWindow();
            if (dashboardWindow != null)
            {
                dashboardWindow.Close();
            }
        }

        private DashboardWindow? GetDashboardWindow()
        {
            return Application.Current.Windows.OfType<DashboardWindow>().FirstOrDefault();
        }

        private void CloseCurrentWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(DashboardWindow))
                    {
                        window.Close();
                    }
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string? _userPhotoUrl;
        public string UserPhotoUrl
        {
            get { return _userPhotoUrl ?? "Не указано"; } // Если _userPhotoUrl равно null, возвращает "Не указано"
            set
            {
                _userPhotoUrl = value;
                OnPropertyChanged(nameof(UserPhotoUrl));
            }
        }

        private string? _userName;
        public string UserName
        {
            get { return _userName ?? "Не указано"; } // Если _userName равно null, возвращает "Не указано"
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string? _userRole;
        public string UserRole
        {
            get { return _userRole ?? "Не указано"; } // Если _userRole равно null, возвращает "Не указано"
            set
            {
                _userRole = value;
                OnPropertyChanged(nameof(UserRole));
            }
        }


        private async Task InitializeUserDataAsync()
        {
            await UserSessionService.Instance.LoadAdditionalUserDataAsync();
            var currentUser = UserSessionService.Instance.CurrentUser;
            if (currentUser != null)
            {
                UserName = currentUser.FullName ?? "Не указано";
                UserRole = currentUser.RoleName ?? "Не указано";
                UserPhotoUrl = currentUser.PhotoUrl ?? "Не указано";
            }
            else
            {
                Console.WriteLine("CurrentUser is null after LoadAdditionalUserData");
            }
        }
    }
}
