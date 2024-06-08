using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using NextGen.src.Services.Api;
using NextGen.src.Services.Security;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views.UserControls;
using static NextGen.src.Services.CustomerService;

namespace NextGen.src.UI.ViewModels
{
    public class AddCustomerDialogViewModel : BaseViewModel
    {
        private string _customerFirstName;
        private string _customerLastName;
        private DateTime _customerDateOfBirth;
        private string _customerPassportNumber;
        private string _customerEmail;
        private string _emailInput;
        private string _displayedEmail;
        private string _selectedEmailSuggestion;
        private ObservableCollection<string> _emailSuggestions;
        private string _customerPhone;
        private string _customerAddress;
        private bool _isFormValid;
        private bool _isDropDownOpen;
        private CustomerService _customerService = new CustomerService();
        private readonly Action<Customer> _addCustomerAction;
        private readonly DadataService _dadataService;
        private ComboBox _emailComboBox;

        public AddCustomerDialogViewModel(Action<Customer> addCustomerAction)
        {
            _addCustomerAction = addCustomerAction;
            _dadataService = new DadataService("45abec0000854b701535bb88095334adfed134b3"); // Ваш API-ключ

            EmailSuggestions = new ObservableCollection<string>();

            AddCustomerCommand = new RelayCommand(AddCustomer, () => IsFormValid);
            CloseDialogCommand = new RelayCommand(CloseDialog);
        }

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

        public string EmailInput
        {
            get => _emailInput;
            set
            {
                if (_emailInput != value)
                {
                    _emailInput = value;
                    OnPropertyChanged();
                    UpdateDisplayedEmail();
                    GetEmailSuggestions(value);
                    ValidateForm(); // Validate form when email input changes
                }
            }
        }

        public string DisplayedEmail
        {
            get => _displayedEmail;
            set
            {
                _displayedEmail = value;
                OnPropertyChanged();
            }
        }

        public string SelectedEmailSuggestion
        {
            get => _selectedEmailSuggestion;
            set
            {
                _selectedEmailSuggestion = value;
                if (!string.IsNullOrEmpty(value))
                {
                    CustomerEmail = value;
                    EmailInput = value;
                    DisplayedEmail = value;
                    IsDropDownOpen = false; // Закрыть выпадающий список после выбора
                }
                OnPropertyChanged();
                ValidateForm(); // Validate form when email suggestion is selected
            }
        }

        public ObservableCollection<string> EmailSuggestions
        {
            get => _emailSuggestions;
            set
            {
                _emailSuggestions = value;
                OnPropertyChanged();
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

        public bool IsDropDownOpen
        {
            get => _isDropDownOpen;
            set
            {
                _isDropDownOpen = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddCustomerCommand { get; }
        public ICommand CloseDialogCommand { get; }

        public void SetEmailComboBox(ComboBox emailComboBox)
        {
            _emailComboBox = emailComboBox;
        }

        private async void AddCustomer()
        {
            var existenceType = _customerService.CheckCustomerExistence(CustomerPassportNumber, CustomerEmail, CustomerPhone);

            if (existenceType != CustomerExistenceType.None)
            {
                string message = existenceType switch
                {
                    CustomerExistenceType.PassportNumber => "Клиент с таким паспортным номером уже существует.",
                    CustomerExistenceType.Email => "Клиент с таким email уже существует.",
                    CustomerExistenceType.Phone => "Клиент с таким телефоном уже существует.",
                    _ => "Клиент с такими данными уже существует."
                };

                var messageDialog = new MessageDialog
                {
                    DataContext = new MessageDialogViewModel
                    {
                        Message = message
                    }
                };
                await DialogHost.Show(messageDialog, "RootDialog");
                return;
            }

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
                          && !string.IsNullOrWhiteSpace(EmailInput) // Validate based on EmailInput
                          && !string.IsNullOrWhiteSpace(CustomerPhone)
                          && !string.IsNullOrWhiteSpace(CustomerAddress);
            ((RelayCommand)AddCustomerCommand).RaiseCanExecuteChanged();
        }

        private async void GetEmailSuggestions(string query)
        {
            Debug.WriteLine($"Getting email suggestions for: {query}");

            if (string.IsNullOrWhiteSpace(query))
            {
                EmailSuggestions.Clear();
                IsDropDownOpen = false;
                return;
            }

            try
            {
                var suggestions = await _dadataService.GetEmailSuggestions(query);

                EmailSuggestions.Clear();
                foreach (var suggestion in suggestions)
                {
                    EmailSuggestions.Add(suggestion);
                }

                IsDropDownOpen = EmailSuggestions.Count > 0;
                UpdateDisplayedEmail();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка при получении подсказок: {ex.Message}");
                IsDropDownOpen = false;
            }
        }

        private void UpdateDisplayedEmail()
        {
            if (EmailSuggestions.Count > 0)
            {
                var suggestion = EmailSuggestions[0];
                if (suggestion.StartsWith(EmailInput, StringComparison.InvariantCultureIgnoreCase))
                {
                    DisplayedEmail = EmailInput + suggestion.Substring(EmailInput.Length);
                }
                else
                {
                    DisplayedEmail = EmailInput;
                }
            }
            else
            {
                DisplayedEmail = EmailInput;
            }
        }

        private int GetCurrentUserId()
        {
            var currentUser = CurrentUser;
            return currentUser?.UserId ?? 0; // Возвращает ID текущего пользователя или 0, если пользователь не найден
        }

        public UserAuthData? CurrentUser => UserSessionService.Instance.CurrentUser;
    }
}
