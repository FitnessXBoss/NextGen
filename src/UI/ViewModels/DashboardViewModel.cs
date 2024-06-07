using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Models;
using NextGen.src.UI.Views;
using NextGen.src.UI.Views.UserControls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NextGen.src.UI.ViewModels
{
    class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly CarService _carService;

        public DashboardViewModel()
        {
            _carService = new CarService(); // Инициализация поля
            InitializeCommands(); // Сначала инициализируйте команды
            InitializeItems();
            FilterItems();

            // Проверьте, инициализированы ли элементы до их использования
            _selectedItem = Items.FirstOrDefault();
            if (_selectedItem != null)
            {
                CurrentContent = _selectedItem.Content;
            }
        }

        // Свойства для команд управления интерфейсом
        public ICommand? ChangeUserCommand { get; private set; }
        public ICommand? MinimizeCommand { get; private set; }
        public ICommand? MaximizeRestoreCommand { get; private set; }
        public ICommand? CloseCommand { get; private set; }
        public ICommand? ToggleThemeCommand { get; private set; }
        public ICommand? ToggleDrawerCommand { get; private set; }
        public ICommand? ToggleRightDrawerCommand { get; private set; }
        public ICommand? CloseUserControlCommand { get; private set; }

        // Коллекции для элементов интерфейса
        public ObservableCollection<DashboardItem> OpenUserControls { get; set; } = new ObservableCollection<DashboardItem>();
        public ObservableCollection<DashboardItem> Items { get; } = new ObservableCollection<DashboardItem>();
        public ObservableCollection<DashboardItem> VisibleItems { get; } = new ObservableCollection<DashboardItem>();
        public ObservableCollection<Model> Models { get; set; } = new ObservableCollection<Model>();

        // Свойства для состояния интерфейса
        private bool _isDrawerOpen;
        private bool _isRightDrawerOpen;
        private UserControl? _currentContent;
        private DashboardItem? _selectedItem;
        private DashboardItem? _selectedUserControl;
        private string _searchKeyword = string.Empty;

        public bool IsDrawerOpen
        {
            get => IsDrawerOpen1;
            set { IsDrawerOpen1 = value; OnPropertyChanged(nameof(IsDrawerOpen)); }
        }

        public bool IsRightDrawerOpen
        {
            get => IsRightDrawerOpen1;
            set { IsRightDrawerOpen1 = value; OnPropertyChanged(nameof(IsRightDrawerOpen)); }
        }

        public UserControl? CurrentContent
        {
            get => CurrentContent1;
            set
            {
                CurrentContent1 = value;
                OnPropertyChanged(nameof(CurrentContent));
            }
        }

        public DashboardItem? SelectedItem
        {
            get => SelectedItem1;
            set
            {
                if (SelectedItem1 != value)
                {
                    ResetSelectionExcept("Main");
                    SelectedItem1 = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    UpdateCurrentContent();
                }
            }
        }

        public DashboardItem? SelectedUserControl
        {
            get => SelectedUserControl1;
            set
            {
                if (SelectedUserControl1 != value)
                {
                    ResetSelectionExcept("Right");
                    SelectedUserControl1 = value;
                    OnPropertyChanged(nameof(SelectedUserControl));
                    UpdateCurrentContent();
                }
            }
        }

        public string SearchKeyword
        {
            get => SearchKeyword1;
            set
            {
                if (SearchKeyword1 != value)
                {
                    SearchKeyword1 = value;
                    OnPropertyChanged(nameof(SearchKeyword));
                    FilterItems();
                }
            }
        }

        public void OpenEmployeeUserControl(UserControl content, string firstName, string lastName, string employeeId)
        {
            string title = $"Редактирование {firstName ?? "не указано"} {lastName ?? "не указано"} [{employeeId}]";
            ActivateOrOpenNewUserControl(content, title);
        }

        public void OpenCarUserControl(UserControl content, string modelName, string brandName, string modelId)
        {
            Console.WriteLine($"Opening Car UserControl with ModelId: {modelId}");  // Для отладки
            string title = $"Автомобили {brandName} {modelName} [{modelId}]";
            ActivateOrOpenNewUserControl(content, title);
        }

        public void OpenCarDetailsControl(UserControl content, string modelName, string brandName, string carId, string trimName)
        {
            Console.WriteLine($"Opening Car Details UserControl with CarId: {carId}, Trim: {trimName}");
            string title = $"Автомобиль {brandName} {modelName} {trimName} [{carId}]";
            ActivateOrOpenNewUserControl(content, title);
        }

        private void ActivateOrOpenNewUserControl(UserControl content, string title)
        {
            var existingItem = OpenUserControls.FirstOrDefault(x => x.Name == title);
            if (existingItem != null)
            {
                SelectedUserControl = existingItem;
                CurrentContent = existingItem.Content;
            }
            else
            {
                var newItem = new DashboardItem { Name = title, Content = content };
                OpenUserControls.Add(newItem);
                SelectedUserControl = newItem;
                CurrentContent = newItem.Content;
            }

            ResetSelectionExcept("Right");
        }





        private void ToggleRightDrawer()
        {
            IsRightDrawerOpen = !IsRightDrawerOpen;
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

        private void InitializeItems()
        {
            Items.Add(new DashboardItem { Name = "Домашняя страница", Content = new HomeControl() });
            Items.Add(new DashboardItem { Name = "Настройки", Content = new SettingsControl() });
            Items.Add(new DashboardItem { Name = "Автомобили", Content = new CarCatalogControl() });
            Items.Add(new DashboardItem { Name = "Сотрудники", Content = new EmployeeControl() });
            Items.Add(new DashboardItem { Name = "Деньги", Content = new MoneyControl() });
            // Добавьте другие элементы по мере необходимости
        }

        private void InitializeCommands()
        {
            ChangeUserCommand = new RelayCommand(ChangeUser);
            MinimizeCommand = new RelayCommand(MinimizeWindow);
            MaximizeRestoreCommand = new RelayCommand(MaximizeRestoreWindow);
            CloseCommand = new RelayCommand(CloseWindow);
            ToggleThemeCommand = new RelayCommand(ToggleTheme);
            ToggleDrawerCommand = new RelayCommand(ToggleDrawer);
            ToggleRightDrawerCommand = new RelayCommand(ToggleRightDrawer);
            CloseUserControlCommand = new RelayCommand<DashboardItem>(ExecuteCloseUserControl, CanExecuteCloseUserControl);
        }

        private void UpdateCurrentContent()
        {
            if (_selectedItem != null) { CurrentContent = _selectedItem.Content; }
            else if (_selectedUserControl != null) { CurrentContent = _selectedUserControl.Content; }
            else { CurrentContent = null; }
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

            if (_selectedItem != null && !VisibleItems.Contains(_selectedItem))
            {
                VisibleItems.Add(_selectedItem);
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

        public static async Task<DashboardViewModel> CreateAsync()
        {
            var viewModel = new DashboardViewModel();
            await viewModel.InitializeUserDataAsync();
            return viewModel;
        }

        private void ToggleDrawer()
        {
            IsDrawerOpen = !IsDrawerOpen;
            OnPropertyChanged(nameof(IsDrawerOpen));
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

        

        // Реализация INotifyPropertyChanged интерфейса
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string? _userPhotoUrl;
        public string UserPhotoUrl
        {
            get { return UserPhotoUrl1 ?? "Не указано"; } // Если _userPhotoUrl равно null, возвращает "Не указано"
            set
            {
                UserPhotoUrl1 = value;
                OnPropertyChanged(nameof(UserPhotoUrl));
            }
        }

        private string? _userName;
        public string UserName
        {
            get { return UserName1 ?? "Не указано"; } // Если _userName равно null, возвращает "Не указано"
            set
            {
                UserName1 = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string? _userRole;
        public string UserRole
        {
            get { return UserRole1 ?? "Не указано"; } // Если _userRole равно null, возвращает "Не указано"
            set
            {
                UserRole1 = value;
                OnPropertyChanged(nameof(UserRole));
            }
        }

        public bool IsDrawerOpen1 { get => _isDrawerOpen; set => _isDrawerOpen = value; }
        public bool IsRightDrawerOpen1 { get => _isRightDrawerOpen; set => _isRightDrawerOpen = value; }
        public UserControl? CurrentContent1 { get => _currentContent; set => _currentContent = value; }
        public DashboardItem? SelectedItem1 { get => _selectedItem; set => _selectedItem = value; }
        public DashboardItem? SelectedUserControl1 { get => _selectedUserControl; set => _selectedUserControl = value; }
        public string SearchKeyword1 { get => _searchKeyword; set => _searchKeyword = value; }
        public string? UserPhotoUrl1 { get => _userPhotoUrl; set => _userPhotoUrl = value; }
        public string? UserName1 { get => _userName; set => _userName = value; }
        public string? UserRole1 { get => _userRole; set => _userRole = value; }

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
