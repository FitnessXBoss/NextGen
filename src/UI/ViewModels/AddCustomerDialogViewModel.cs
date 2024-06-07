using System;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.Helpers;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;

namespace NextGen.src.UI.ViewModels
{
    public class AddCustomerDialogViewModel : BaseViewModel
    {
        private string _customerFirstName;
        private string _customerLastName;
        private string _customerEmail;
        private string _customerPhone;
        private CustomerService _customerService = new CustomerService();
        private readonly Action<Customer> _addCustomerAction;

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

        private bool _isFormValid;
        public bool IsFormValid
        {
            get => _isFormValid;
            private set
            {
                _isFormValid = value;
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand CloseDialogCommand { get; }

        public AddCustomerDialogViewModel(Action<Customer> addCustomerAction)
        {
            _addCustomerAction = addCustomerAction;
            AddCustomerCommand = new RelayCommand(AddCustomer, () => IsFormValid);
            CloseDialogCommand = new RelayCommand(CloseDialog);
        }

        private void AddCustomer()
        {
            var newCustomer = new Customer
            {
                FirstName = CustomerFirstName,
                LastName = CustomerLastName,
                Email = CustomerEmail,
                Phone = CustomerPhone
            };

            newCustomer = _customerService.AddCustomer(newCustomer);
            _addCustomerAction(newCustomer);
            DialogHost.CloseDialogCommand.Execute(true, null);
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
