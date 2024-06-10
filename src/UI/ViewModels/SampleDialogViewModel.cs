using MaterialDesignThemes.Wpf;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Models;
using NextGen.src.UI.Views;
using NextGen.src.UI.Views.UserControls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NextGen.src.UI.ViewModels
{
    public class SampleDialogViewModel : BaseViewModel
    {
        private readonly CarService _carService;  // Добавляем CarService
        private readonly int _carId; // Добавляем поле для хранения CarId
        private string _customerFirstName;
        private string _customerLastName;
        private string _customerEmail;
        private string _customerPhone;
        private Customer _selectedCustomer;
        private bool _isFormValid;
        private string _filterText;
        private ObservableCollection<Customer> _customers;
        private ObservableCollection<Customer> _filteredCustomers;
        private CustomerService _customerService = new CustomerService();

        public string CustomerFirstName
        {
            get => _customerFirstName;
            set
            {
                _customerFirstName = value;
                OnPropertyChanged(nameof(CustomerFirstName));
                ValidateForm();
            }
        }

        public string CustomerLastName
        {
            get => _customerLastName;
            set
            {
                _customerLastName = value;
                OnPropertyChanged(nameof(CustomerLastName));
                ValidateForm();
            }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set
            {
                _customerEmail = value;
                OnPropertyChanged(nameof(CustomerEmail));
                ValidateForm();
            }
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set
            {
                _customerPhone = value;
                OnPropertyChanged(nameof(CustomerPhone));
                ValidateForm();
            }
        }

        public int CustomerId => _selectedCustomer?.Id ?? 0;

        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
                if (_selectedCustomer != null)
                {
                    CustomerFirstName = _selectedCustomer.FirstName;
                    CustomerLastName = _selectedCustomer.LastName;
                    CustomerEmail = _selectedCustomer.Email;
                    CustomerPhone = _selectedCustomer.Phone;

                    // Сохранение выбранного клиента в TempDataStore
                    TempDataStore.SelectedCustomer = _selectedCustomer;

                    // Добавьте отладочную информацию
                    Debug.WriteLine($"SelectedCustomer set with CarId: {_selectedCustomer.CarId}");
                }
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                _filterText = value;
                OnPropertyChanged(nameof(FilterText));
                ApplyFilter();
            }
        }

        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set
            {
                _customers = value;
                OnPropertyChanged(nameof(Customers));
                ApplyFilter();
            }
        }

        public ObservableCollection<Customer> FilteredCustomers
        {
            get => _filteredCustomers;
            set
            {
                _filteredCustomers = value;
                OnPropertyChanged(nameof(FilteredCustomers));
            }
        }

        public bool IsFormValid
        {
            get => _isFormValid;
            private set
            {
                _isFormValid = value;
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public ICommand ConfirmSellCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand CloseDialogCommand { get; }
        public ICommand OpenSalesContractCommand { get; } // Добавляем команду

        public SampleDialogViewModel(int carId) // Обновляем конструктор для принятия CarId
        {
            _carId = carId; // Сохраняем CarId
            _carService = new CarService(); // Инициализируем CarService
            ConfirmSellCommand = new RelayCommand(ConfirmSell, () => IsFormValid);
            AddCustomerCommand = new RelayCommand(async () => await ExecuteAddCustomerDialogAsync());
            CloseDialogCommand = new RelayCommand(CloseDialog);
            OpenSalesContractCommand = new RelayCommand(OpenSalesContractDialog);

            Customers = new ObservableCollection<Customer>();
            FilteredCustomers = new ObservableCollection<Customer>();
            LoadCustomers();
        }

        private async void LoadCustomers()
        {
            var customers = await Task.Run(() => _customerService.GetAllCustomers());
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
        }

        public void AddNewCustomer(Customer newCustomer)
        {
            Customers.Add(newCustomer);
            SelectedCustomer = newCustomer;

            // Добавьте отладочную информацию здесь
            Debug.WriteLine($"New Customer Added: {newCustomer.FirstName} {newCustomer.LastName}, CarId: {newCustomer.CarId}");

            // Обновление свойств, чтобы ComboBox отобразил правильные значения
            CustomerFirstName = newCustomer.FirstName;
            CustomerLastName = newCustomer.LastName;
            CustomerEmail = newCustomer.Email;
            CustomerPhone = newCustomer.Phone;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (string.IsNullOrWhiteSpace(FilterText))
            {
                FilteredCustomers = new ObservableCollection<Customer>(Customers);
            }
            else
            {
                var lowerCaseFilter = FilterText.ToLower();
                var filteredList = Customers.Where(c =>
                    c.FirstName.ToLower().Contains(lowerCaseFilter) ||
                    c.LastName.ToLower().Contains(lowerCaseFilter) ||
                    c.Email.ToLower().Contains(lowerCaseFilter) ||
                    c.Phone.ToLower().Contains(lowerCaseFilter)).ToList();
                FilteredCustomers = new ObservableCollection<Customer>(filteredList);
            }
        }

        private async Task ExecuteAddCustomerDialogAsync()
        {
            var view = new AddCustomerDialog
            {
                DataContext = new AddCustomerDialogViewModel(AddNewCustomer)
            };

            await DialogHost.Show(view, "AddCustomerDialogHost");
        }

        private void CloseDialog()
        {
            DialogHost.CloseDialogCommand.Execute(false, null);
        }

        private void ValidateForm()
        {
            IsFormValid = !string.IsNullOrWhiteSpace(CustomerFirstName)
                          && !string.IsNullOrWhiteSpace(CustomerLastName)
                          && !string.IsNullOrWhiteSpace(CustomerEmail)
                          && !string.IsNullOrWhiteSpace(CustomerPhone);
        }

        private void ConfirmSell()
        {
            if (SelectedCustomer != null)
            {
                TempDataStore.SelectedCustomer = SelectedCustomer;
                TempDataStore.CarDetails = GetCarDetails();  // Получаем объект CarDetails

                Debug.WriteLine($"SelectedCustomer: {SelectedCustomer.FullName}, PassportNumber: {SelectedCustomer.PassportNumber}, PassportIssueDate: {SelectedCustomer.PassportIssueDate}, PassportIssuer: {SelectedCustomer.PassportIssuer}, Address: {SelectedCustomer.Address}, CustomerPhone: {SelectedCustomer.Phone}");
                OpenSalesContractDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента перед продажей.");
            }
        }


        private CarDetails GetCarDetails()
        {
            return _carService.GetCarDetails(_carId);
        }

        private void OpenSalesContractDialog()
        {
            var carId = _carId; // Используем сохраненный carId
            Debug.WriteLine($"CarId from SelectedCustomer: {carId}");

            var view = new SalesContractDialog
            {
                DataContext = new SalesContractViewModel(carId) // Передаем carId
            };

            var dashboardViewModel = Application.Current.Windows.OfType<DashboardWindow>().FirstOrDefault()?.DataContext as DashboardViewModel;
            if (dashboardViewModel != null)
            {
                dashboardViewModel.OpenSalesContractControl(view, SelectedCustomer.FirstName, SelectedCustomer.LastName, SelectedCustomer.Id, carId);
            }
        }
    }
}
