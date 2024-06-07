using System.Collections.ObjectModel;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.Helpers;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using System.Threading.Tasks;
using NextGen.src.UI.Views.UserControls;

namespace NextGen.src.UI.ViewModels
{
    public class SampleDialogViewModel : BaseViewModel
    {
        private string _customerFirstName;
        private string _customerLastName;
        private string _customerEmail;
        private string _customerPhone;
        private Customer _selectedCustomer;
        private bool _isFormValid;
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
                }
            }
        }

        public ObservableCollection<Customer> Customers { get; set; }

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

        public SampleDialogViewModel()
        {
            ConfirmSellCommand = new RelayCommand(ConfirmSell, () => IsFormValid);
            AddCustomerCommand = new RelayCommand(async () => await ExecuteAddCustomerDialogAsync());
            CloseDialogCommand = new RelayCommand(CloseDialog);
            Customers = new ObservableCollection<Customer>();
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
            // Обновление свойств, чтобы ComboBox отобразил правильные значения
            CustomerFirstName = newCustomer.FirstName;
            CustomerLastName = newCustomer.LastName;
            CustomerEmail = newCustomer.Email;
            CustomerPhone = newCustomer.Phone;
        }

        private void ConfirmSell()
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
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
    }
}
