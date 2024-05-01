using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using NextGen.src.Services;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views;
using NextGen.src.Services.Security;
using System.Windows.Controls;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using NextGen.src.UI.Views.UserControls;
using NextGen.src.UI.Models; 
using System.Collections.ObjectModel;

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

        private DashboardItem? _selectedItem;  // Явно указываем, что переменная может быть null

        public ObservableCollection<DashboardItem> Items { get; } = new ObservableCollection<DashboardItem>();

        public DashboardItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    OnPropertyChanged(nameof(CurrentContent));
                }
            }
        }


        public UserControl? CurrentContent => SelectedItem?.Content;

        private void InitializeItems()
        {
            Items.Add(new DashboardItem { Name = "Домашняя страница", Content = new HomeControl() });
            Items.Add(new DashboardItem { Name = "Настройки", Content = new SettingsControl() });
            // Добавьте другие элементы по мере необходимости
        }

        private DashboardViewModel()
        {
            InitializeItems();
            FilterItems();  // Обязательно вызовите FilterItems после инициализации элементов

            ChangeUserCommand = new RelayCommand(ChangeUser);
            MinimizeCommand = new RelayCommand(MinimizeWindow);
            MaximizeRestoreCommand = new RelayCommand(MaximizeRestoreWindow);
            CloseCommand = new RelayCommand(CloseWindow);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ToggleDrawerCommand = new RelayCommand(ToggleDrawer);

            _selectedItem = null; // Явное установление начального значения как null
        }


        public ObservableCollection<DashboardItem> VisibleItems { get; } = new ObservableCollection<DashboardItem>();

        private string _searchKeyword = string.Empty;

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword != value)
                {
                    _searchKeyword = value;
                    OnPropertyChanged(nameof(SearchKeyword));
                    FilterItems();
                }
            }
        }

        private void FilterItems()
        {
            // Показать все элементы, если строка поиска пуста
            var filtered = string.IsNullOrEmpty(SearchKeyword) ?
                Items.ToList() :
                Items.Where(item => item.Name?.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase) ?? false).ToList();

            VisibleItems.Clear();
            foreach (var item in filtered)
            {
                VisibleItems.Add(item);
            }
        }

        public static async Task<DashboardViewModel> CreateAsync()
        {
            var viewModel = new DashboardViewModel();
            await viewModel.InitializeUserDataAsync();
            return viewModel;
        }

        private void ToggleDrawer()
        {
            IsDrawerOpen = !IsDrawerOpen;
        }

        private void ToggleTheme()
        {
            var themeService = new ThemeService();
            themeService.ToggleTheme();
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
