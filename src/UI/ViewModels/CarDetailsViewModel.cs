using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using NextGen.src.Services.Security;
using NextGen.src.UI.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace NextGen.src.UI.ViewModels
{
    public class CarDetailsViewModel : BaseViewModel
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

        private string _carColor;
        private int _carYear;
        private string _carAdditionalFeatures;
        private string _carTransmission;
        private string _carDrive;
        private string _carFuel;
        private string _carEngineVolume;
        private string _carHorsePower;
        private decimal _carPrice;
        private string _carTrimDetails;
        private int _carSeats;
        private string _carLength;
        private string _carWidth;
        private string _carHeight;
        private string _carTrunkVolume;
        private string _carFuelTankVolume;
        private string _carMixedConsumption;
        private string _carCityConsumption;
        private string _carHighwayConsumption;
        private string _carMaxSpeed;
        private string _carAcceleration;
        private string _carBodyType;

        public string CarColor
        {
            get => _carColor;
            set
            {
                _carColor = value;
                OnPropertyChanged(nameof(CarColor));
            }
        }

        public int CarYear
        {
            get => _carYear;
            set
            {
                _carYear = value;
                OnPropertyChanged(nameof(CarYear));
            }
        }

        public string CarAdditionalFeatures
        {
            get => _carAdditionalFeatures;
            set
            {
                _carAdditionalFeatures = value;
                OnPropertyChanged(nameof(CarAdditionalFeatures));
            }
        }

        public string CarTransmission
        {
            get => _carTransmission;
            set
            {
                _carTransmission = value;
                OnPropertyChanged(nameof(CarTransmission));
            }
        }

        public string CarDrive
        {
            get => _carDrive;
            set
            {
                _carDrive = value;
                OnPropertyChanged(nameof(CarDrive));
            }
        }

        public string CarFuel
        {
            get => _carFuel;
            set
            {
                _carFuel = value;
                OnPropertyChanged(nameof(CarFuel));
            }
        }

        public string CarEngineVolume
        {
            get => _carEngineVolume;
            set
            {
                _carEngineVolume = value;
                OnPropertyChanged(nameof(CarEngineVolume));
            }
        }

        public string CarHorsePower
        {
            get => _carHorsePower;
            set
            {
                _carHorsePower = value;
                OnPropertyChanged(nameof(CarHorsePower));
            }
        }

        public decimal CarPrice
        {
            get => _carPrice;
            set
            {
                _carPrice = value;
                OnPropertyChanged(nameof(CarPrice));
            }
        }

        public string CarTrimDetails
        {
            get => _carTrimDetails;
            set
            {
                _carTrimDetails = value;
                OnPropertyChanged(nameof(CarTrimDetails));
            }
        }

        public int CarSeats
        {
            get => _carSeats;
            set
            {
                _carSeats = value;
                OnPropertyChanged(nameof(CarSeats));
            }
        }

        public string CarLength
        {
            get => _carLength;
            set
            {
                _carLength = value;
                OnPropertyChanged(nameof(CarLength));
            }
        }

        public string CarWidth
        {
            get => _carWidth;
            set
            {
                _carWidth = value;
                OnPropertyChanged(nameof(CarWidth));
            }
        }

        public string CarHeight
        {
            get => _carHeight;
            set
            {
                _carHeight = value;
                OnPropertyChanged(nameof(CarHeight));
            }
        }

        public string CarTrunkVolume
        {
            get => _carTrunkVolume;
            set
            {
                _carTrunkVolume = value;
                OnPropertyChanged(nameof(CarTrunkVolume));
            }
        }

        public string CarFuelTankVolume
        {
            get => _carFuelTankVolume;
            set
            {
                _carFuelTankVolume = value;
                OnPropertyChanged(nameof(CarFuelTankVolume));
            }
        }

        public string CarMixedConsumption
        {
            get => _carMixedConsumption;
            set
            {
                _carMixedConsumption = value;
                OnPropertyChanged(nameof(CarMixedConsumption));
            }
        }

        public string CarCityConsumption
        {
            get => _carCityConsumption;
            set
            {
                _carCityConsumption = value;
                OnPropertyChanged(nameof(CarCityConsumption));
            }
        }

        public string CarHighwayConsumption
        {
            get => _carHighwayConsumption;
            set
            {
                _carHighwayConsumption = value;
                OnPropertyChanged(nameof(CarHighwayConsumption));
            }
        }

        public string CarMaxSpeed
        {
            get => _carMaxSpeed;
            set
            {
                _carMaxSpeed = value;
                OnPropertyChanged(nameof(CarMaxSpeed));
            }
        }

        public string CarAcceleration
        {
            get => _carAcceleration;
            set
            {
                _carAcceleration = value;
                OnPropertyChanged(nameof(CarAcceleration));
            }
        }

        public string CarBodyType
        {
            get => _carBodyType;
            set
            {
                _carBodyType = value;
                OnPropertyChanged(nameof(CarBodyType));
            }
        }

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

            CarColor = details.Color;
            CarYear = details.Year;
            CarAdditionalFeatures = details.AdditionalFeatures;
            CarTransmission = details.Transmission;
            CarDrive = details.Drive;
            CarFuel = details.Fuel;
            CarEngineVolume = details.EngineVolume;
            CarHorsePower = details.HorsePower;
            CarPrice = details.Price;
            CarTrimDetails = details.TrimDetails;
            CarSeats = details.Seats;
            CarLength = details.Length;
            CarWidth = details.Width;
            CarHeight = details.Height;
            CarTrunkVolume = details.TrunkVolume;
            CarFuelTankVolume = details.FuelTankVolume;
            CarMixedConsumption = details.MixedConsumption;
            CarCityConsumption = details.CityConsumption;
            CarHighwayConsumption = details.HighwayConsumption;
            CarMaxSpeed = details.MaxSpeed;
            CarAcceleration = details.Acceleration;
            CarBodyType = details.CarBodyType;
        }

        private void SellCar()
        {
            _carService.SellCar(_carId, GetCurrentUserId(), CustomerId, CustomerFirstName, CustomerLastName, CustomerEmail, CustomerPhone);
            // Обновите статус автомобиля в представлении, если необходимо
        }

        private int GetCurrentUserId()
        {
            var currentUser = CurrentUser;
            return currentUser?.UserId ?? 0; // Возвращает ID текущего пользователя или 0, если пользователь не найден
        }

        public UserAuthData? CurrentUser => UserSessionService.Instance.CurrentUser;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
