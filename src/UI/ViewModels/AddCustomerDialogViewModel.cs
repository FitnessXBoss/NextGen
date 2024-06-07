using System;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.Helpers;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using NextGen.src.Services.Security;

namespace NextGen.src.UI.ViewModels
{
    public class AddCustomerDialogViewModel : BaseViewModel
    {
        private string _customerFirstName;
        private string _customerLastName;
        private DateTime _customerDateOfBirth;
        private string _customerPassportNumber;
        private string _customerEmail;
        private string _customerPhone;
        private string _customerAddress;
        private bool _isFormValid;
        private CustomerService _customerService = new CustomerService();
        private readonly Action<Customer> _addCustomerAction;

        public string CustomerFirstName
        {
            get => _customerFirstName;
            set
            {
                _customerFirstName = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string CustomerLastName
        {
            get => _customerLastName;
            set
            {
                _customerLastName = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public DateTime CustomerDateOfBirth
        {
            get => _customerDateOfBirth;
            set
            {
                _customerDateOfBirth = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string CustomerPassportNumber
        {
            get => _customerPassportNumber;
            set
            {
                _customerPassportNumber = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set
            {
                _customerEmail = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set
            {
                _customerPhone = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public string CustomerAddress
        {
            get => _customerAddress;
            set
            {
                _customerAddress = value;
                OnPropertyChanged();
                ValidateForm();
            }
        }

        public bool IsFormValid
        {
            get => _isFormValid;
            private set
            {
                _isFormValid = value;
                OnPropertyChanged();
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
                DateOfBirth = CustomerDateOfBirth,
                PassportNumber = CustomerPassportNumber,
                Email = CustomerEmail,
                Phone = CustomerPhone,
                Address = CustomerAddress
            };

            int createdBy = GetCurrentUserId();
            newCustomer = _customerService.AddCustomer(newCustomer, createdBy);
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
                          && CustomerDateOfBirth != default
                          && !string.IsNullOrWhiteSpace(CustomerPassportNumber)
                          && !string.IsNullOrWhiteSpace(CustomerEmail)
                          && !string.IsNullOrWhiteSpace(CustomerPhone)
                          && !string.IsNullOrWhiteSpace(CustomerAddress);
        }

        private int GetCurrentUserId()
        {
            var currentUser = CurrentUser;
            return currentUser?.UserId ?? 0; // Возвращает ID текущего пользователя или 0, если пользователь не найден
        }

        public UserAuthData? CurrentUser => UserSessionService.Instance.CurrentUser;
    }
}
