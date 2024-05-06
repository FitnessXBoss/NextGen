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
        public ICommand ToggleRightDrawerCommand { get; private set; }
        public ICommand CloseUserControlCommand { get; private set; }




        public ObservableCollection<DashboardItem> OpenUserControls { get; set; } = new ObservableCollection<DashboardItem>();


        public void OpenUserControl(UserControl content, string? firstName, string? lastName, string employeeId)
        {
            string title = $"Редактирование {firstName ?? "не указано"} {lastName ?? "не указано"} [{employeeId}]";
            var newItem = new DashboardItem { Name = title, Content = content };
            OpenUserControls.Add(newItem);
            SelectedUserControl = newItem;
            CurrentContent = newItem.Content;
            ResetSelectionExcept("Right");
        }







        private void ToggleRightDrawer()
        {
            IsRightDrawerOpen = !IsRightDrawerOpen;
        }


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




        

        private bool _isRightDrawerOpen;
        public bool IsRightDrawerOpen
        {
            get => _isRightDrawerOpen;
            set
            {
                _isRightDrawerOpen = value;
                OnPropertyChanged(nameof(IsRightDrawerOpen));
            }
        }


       
        private UserControl? _currentContent; // Явно указываем, что переменная может быть null

        public ObservableCollection<DashboardItem> Items { get; } = new ObservableCollection<DashboardItem>();
        public ObservableCollection<DashboardItem> VisibleItems { get; } = new ObservableCollection<DashboardItem>();



        private DashboardItem? _selectedItem;
        public DashboardItem? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    ResetSelectionExcept("Main");  // Передаем имя текущего списка
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    UpdateCurrentContent();
                }
            }
        }

        private DashboardItem? _selectedUserControl;
        public DashboardItem? SelectedUserControl
        {
            get => _selectedUserControl;
            set
            {
                if (_selectedUserControl != value)
                {
                    ResetSelectionExcept("Right");  // Сбросить выбор в другом списке
                    _selectedUserControl = value;
                    OnPropertyChanged(nameof(SelectedUserControl));
                    UpdateCurrentContent(); // Обновить текущий контент при изменении выбранного UserControl
                }
            }
        }



        public void ResetSelectionExcept(string listName)
        {
            if (listName != "Main")
            {
                SelectedItem = null;  // Сбрасываем выбор в основном списке
            }
            if (listName != "Right")
            {
                SelectedUserControl = null;  // Сбрасываем выбор в правом списке
            }
        }




        public UserControl? CurrentContent
        {
            get => _currentContent;
            set
            {
                if (_currentContent != value)
                {
                    _currentContent = value;
                    OnPropertyChanged(nameof(CurrentContent));
                }
            }
        }


        private void UpdateCurrentContent()
        {
            // Установка CurrentContent в зависимости от того, какой элемент сейчас выбран
            if (_selectedItem != null)
            {
                CurrentContent = _selectedItem.Content; // Контент из основного списка
            }
            else if (_selectedUserControl != null)
            {
                CurrentContent = _selectedUserControl.Content; // Контент из списка открытых UserControl'ов
            }
            else
            {
                CurrentContent = null; // Сброс, если ничего не выбрано
            }
        }

        private void FilterItems()
        {
            var filtered = string.IsNullOrEmpty(SearchKeyword) ?
                Items.ToList() :
                Items.Where(item => item.Name?.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase) ?? false).ToList();

            VisibleItems.Clear();
            foreach (var item in filtered)
            {
                VisibleItems.Add(item);
            }

            // Если текущий выбранный элемент не виден, но должен быть сохранен
            if (_selectedItem != null && !VisibleItems.Contains(_selectedItem))
            {
                VisibleItems.Add(_selectedItem);
            }
        }



        

        private void InitializeItems()
        {
            Items.Add(new DashboardItem { Name = "Домашняя страница", Content = new HomeControl() });
            Items.Add(new DashboardItem { Name = "Настройки", Content = new SettingsControl() });
            Items.Add(new DashboardItem { Name = "Сотрудники", Content = new EmployeeControl() });
            // Добавьте другие элементы по мере необходимости
        }

        private DashboardViewModel()
        {
            InitializeItems(); // Инициализация элементов
            FilterItems();     // Фильтрация элементов по необходимости

            ChangeUserCommand = new RelayCommand(ChangeUser);
            MinimizeCommand = new RelayCommand(MinimizeWindow);
            MaximizeRestoreCommand = new RelayCommand(MaximizeRestoreWindow);
            CloseCommand = new RelayCommand(CloseWindow);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ToggleDrawerCommand = new RelayCommand(ToggleDrawer);
            ToggleRightDrawerCommand = new RelayCommand(ToggleRightDrawer);
            CloseUserControlCommand = new RelayCommand<DashboardItem>(ExecuteCloseUserControl, CanExecuteCloseUserControl);


            // Установите начальный выбор элемента и контента
            _selectedItem = Items.FirstOrDefault(); // Устанавливаем первый элемент коллекции Items как выбранный
            if (_selectedItem != null)
            {
                CurrentContent = _selectedItem.Content; // Устанавливаем содержимое выбранного элемента
            }
        }

        private bool CanExecuteCloseUserControl(DashboardItem item)
        {
            // Можно добавить логику для определения, можно ли закрыть элемент
            return item != null;
        }

        private void ExecuteCloseUserControl(DashboardItem item)
        {
            if (item != null && OpenUserControls.Contains(item))
            {
                OpenUserControls.Remove(item);
                if (SelectedUserControl == item)
                {
                    SelectedUserControl = null;
                    CurrentContent = null;
                }
            }
        }




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
