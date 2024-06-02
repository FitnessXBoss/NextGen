using System.ComponentModel;
using System.Windows.Input;
using NextGen.src.UI.Helpers;

namespace NextGen.src.UI.ViewModels
{
    public class SampleDialogViewModel : INotifyPropertyChanged
    {
        // Ваши свойства и методы для SampleDialog
        private string _customerId;
        public string CustomerId
        {
            get => _customerId;
            set
            {
                _customerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }

        // Другие свойства для SampleDialog
        private string _customerFirstName;
        public string CustomerFirstName
        {
            get => _customerFirstName;
            set
            {
                _customerFirstName = value;
                OnPropertyChanged(nameof(CustomerFirstName));
            }
        }

        private string _customerLastName;
        public string CustomerLastName
        {
            get => _customerLastName;
            set
            {
                _customerLastName = value;
                OnPropertyChanged(nameof(CustomerLastName));
            }
        }

        private string _customerEmail;
        public string CustomerEmail
        {
            get => _customerEmail;
            set
            {
                _customerEmail = value;
                OnPropertyChanged(nameof(CustomerEmail));
            }
        }

        private string _customerPhone;
        public string CustomerPhone
        {
            get => _customerPhone;
            set
            {
                _customerPhone = value;
                OnPropertyChanged(nameof(CustomerPhone));
            }
        }

        private bool _isAgreeToPersonalDataProcessing;
        public bool IsAgreeToPersonalDataProcessing
        {
            get => _isAgreeToPersonalDataProcessing;
            set
            {
                _isAgreeToPersonalDataProcessing = value;
                OnPropertyChanged(nameof(IsAgreeToPersonalDataProcessing));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        private bool _isAgreeToReceiveOffers;
        public bool IsAgreeToReceiveOffers
        {
            get => _isAgreeToReceiveOffers;
            set
            {
                _isAgreeToReceiveOffers = value;
                OnPropertyChanged(nameof(IsAgreeToReceiveOffers));
            }
        }

        private bool _isAgreeToCreditTerms;
        public bool IsAgreeToCreditTerms
        {
            get => _isAgreeToCreditTerms;
            set
            {
                _isAgreeToCreditTerms = value;
                OnPropertyChanged(nameof(IsAgreeToCreditTerms));
                OnPropertyChanged(nameof(IsFormValid));
            }
        }

        public bool IsFormValid => IsAgreeToPersonalDataProcessing && IsAgreeToCreditTerms;

        public ICommand SellCarCommand { get; }

        public SampleDialogViewModel()
        {
            SellCarCommand = new RelayCommand(SellCar, () => IsFormValid);
        }

        private void SellCar()
        {
            // Логика продажи машины
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
