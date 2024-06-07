using System.Collections.ObjectModel;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.Helpers;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;

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
        private CustomerService _customerService = new CustomerService(); // Создание экземпляра CustomerService

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

        public int CustomerId => _selectedCustomer?.Id ?? 0; // Получение CustomerId

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

        public SampleDialogViewModel()
        {
            ConfirmSellCommand = new RelayCommand(ConfirmSell, () => IsFormValid);
            LoadCustomers();
        }

        private void LoadCustomers()
        {
            var customers = _customerService.GetAllCustomers(); // Загрузка клиентов из базы данных
            Customers = new ObservableCollection<Customer>(customers);
        }

        private void ConfirmSell()
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
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
