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

        public DashboardViewModel()
        {
            ChangeUserCommand = new RelayCommand(ChangeUser);
            MinimizeCommand = new RelayCommand(MinimizeWindow);
            MaximizeRestoreCommand = new RelayCommand(MaximizeRestoreWindow);
            CloseCommand = new RelayCommand(CloseWindow);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ToggleDrawerCommand = new RelayCommand(ToggleDrawer);

            InitializeUserDataAsync(); 
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
            try
            {
                var dashboardWindow = GetDashboardWindow();
                dashboardWindow.WindowState = WindowState.Minimized;
            }
            catch (InvalidOperationException ex)
            {
                // Обрабатываем ошибку (например, записываем в лог)
                Console.WriteLine(ex.Message);
            }
        }

        private void MaximizeRestoreWindow()
        {
            try
            {
                var dashboardWindow = GetDashboardWindow();
                dashboardWindow.WindowState = dashboardWindow.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            catch (InvalidOperationException ex)
            {
                // Обрабатываем ошибку
                Console.WriteLine(ex.Message);
            }
        }

        private void CloseWindow()
        {
            try
            {
                var dashboardWindow = GetDashboardWindow();
                dashboardWindow.Close();
            }
            catch (InvalidOperationException ex)
            {
                // Обрабатываем ошибку
                Console.WriteLine(ex.Message);
            }
        }

        private DashboardWindow GetDashboardWindow()
        {
            var dashboardWindow = Application.Current.Windows
                .OfType<DashboardWindow>()
                .FirstOrDefault();

            if (dashboardWindow == null)
                throw new InvalidOperationException("DashboardWindow не найдено.");

            return dashboardWindow;
        }


        private void ToggleTheme()
        {
            // Логика для изменения темы
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


        private string _userPhotoUrl;
        public string UserPhotoUrl
        {
            get { return _userPhotoUrl; }
            set
            {
                _userPhotoUrl = value;
                OnPropertyChanged(nameof(UserPhotoUrl));
            }
        }

        private string _userName;
        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string _userRole;
        public string UserRole
        {
            get { return _userRole; }
            set
            {
                _userRole = value;
                OnPropertyChanged(nameof(UserRole));
            }
        }

        // Метод для инициализации данных пользователя
        public async Task InitializeUserDataAsync()
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