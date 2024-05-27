using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using NextGen.src.UI.Helpers;

namespace NextGen.src.UI.ViewModels
{
    public class CarDetailsViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<CarImage> CarImages { get; set; }
        private CarService _carService = new CarService();
        private int _carId;

        private string _customerFirstName;
        private string _customerLastName;
        private string _customerEmail;
        private string _customerPhone;
        private bool _isAgreeToPersonalDataProcessing;
        private bool _isAgreeToReceiveOffers;
        private bool _isAgreeToCreditTerms;

        public int CustomerId { get; set; }
        public ICommand SellCarCommand { get; }

        public CarDetailsViewModel(int carId)
        {
            _carId = carId;
            LoadDetailsForCar(_carId);
            SellCarCommand = new RelayCommand(SellCar);
        }

        public bool IsAgreeToPersonalDataProcessing
        {
            get => _isAgreeToPersonalDataProcessing;
            set
            {
                _isAgreeToPersonalDataProcessing = value;
                OnPropertyChanged(nameof(IsAgreeToPersonalDataProcessing));
                UpdateFormValidity();
            }
        }

        public bool IsAgreeToReceiveOffers
        {
            get => _isAgreeToReceiveOffers;
            set
            {
                _isAgreeToReceiveOffers = value;
                OnPropertyChanged(nameof(IsAgreeToReceiveOffers));
                UpdateFormValidity();
            }
        }

        public bool IsAgreeToCreditTerms
        {
            get => _isAgreeToCreditTerms;
            set
            {
                _isAgreeToCreditTerms = value;
                OnPropertyChanged(nameof(IsAgreeToCreditTerms));
                UpdateFormValidity();
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

        private void UpdateFormValidity()
        {
            IsFormValid = IsAgreeToPersonalDataProcessing && IsAgreeToCreditTerms; // Пример условия валидации формы
        }


        public string CustomerFirstName
        {
            get => _customerFirstName;
            set
            {
                _customerFirstName = value;
                OnPropertyChanged(nameof(CustomerFirstName));
            }
        }

        public string CustomerLastName
        {
            get => _customerLastName;
            set
            {
                _customerLastName = value;
                OnPropertyChanged(nameof(CustomerLastName));
            }
        }

        public string CustomerEmail
        {
            get => _customerEmail;
            set
            {
                _customerEmail = value;
                OnPropertyChanged(nameof(CustomerEmail));
            }
        }

        public string CustomerPhone
        {
            get => _customerPhone;
            set
            {
                _customerPhone = value;
                OnPropertyChanged(nameof(CustomerPhone));
            }
        }

        private void LoadDetailsForCar(int carId)
        {
            CarDetails details = _carService.GetCarDetails(carId);
            CarImages = new ObservableCollection<CarImage> { new CarImage { ImagePath = details.ImageUrl } };
            foreach (var img in details.Images)
            {
                CarImages.Add(img);
            }
        }

        private void SellCar()
        {
            _carService.SellCar(_carId, GetCurrentUserId(), CustomerId, CustomerFirstName, CustomerLastName, CustomerEmail, CustomerPhone);
            // Обновите статус автомобиля в представлении, если необходимо
        }

        private int GetCurrentUserId()
        {
            // Здесь должен быть ваш код для получения ID текущего пользователя
            return 1; // Заглушка для примера
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
