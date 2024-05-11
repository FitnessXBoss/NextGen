using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using NextGen.src.Data.Database.Models;  // Предполагаемое местоположение класса Car
using NextGen.src.Services;
using System.Diagnostics;  // Предполагаемое местоположение CarService
using System.Windows.Input;
using NextGen.src.UI.Helpers;


namespace NextGen.src.UI.ViewModels
{
    public class CarsViewModel : INotifyPropertyChanged
    {
        public ICommand RefreshDataCommand { get; private set; }
        public ICommand LoadBrandsCommand { get; private set; }

        public ObservableCollection<Brand> Brands { get; set; } = new ObservableCollection<Brand>();
        public ObservableCollection<Model> Models { get; set; } = new ObservableCollection<Model>();

        private Brand _selectedBrand = null!;
        public Brand SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (_selectedBrand != value)
                {
                    _selectedBrand = value;
                    OnPropertyChanged();
                    if (value != null)
                    {
                        LoadModelsForBrand(value.BrandId);
                        FilterCars(); // Фильтрация при изменении выбранного бренда
                    }
                    else
                    {
                        Models.Clear(); // Очистка моделей, если бренд не выбран
                        FilterCars(); // Перефильтрация автомобилей без учета бренда
                    }
                }
            }
        }


        private Model _selectedModel = null!;
        public Model SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    OnPropertyChanged();
                    // Фильтрация автомобилей даже если модель не выбрана (например, если выбор модели очищен)
                    FilterCars();
                }
            }
        }


       

        private async void LoadModelsForBrand(int brandId)
        {
            var models = await Task.Run(() => _carService.GetModelsByBrand(brandId));
            Models.Clear();
            foreach (var model in models)
            {
                Models.Add(model);
            }
        }


        private async Task LoadBrandsAsync()
        {
            try
            {
                var brands = await Task.Run(() => _carService.GetAllBrands());
                Brands.Clear();
                foreach (var brand in brands)
                {
                    Brands.Add(brand);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading brands: " + ex.Message);
            }
        }


        private ObservableCollection<Car> _cars = new ObservableCollection<Car>();
        public ObservableCollection<Car> Cars
        {
            get => _cars;
            set
            {
                _cars = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Car> _filteredCars = new ObservableCollection<Car>();
        public ObservableCollection<Car> FilteredCars
        {
            get => _filteredCars;
            set
            {
                _filteredCars = value;
                OnPropertyChanged();
            }
        }

        

        private readonly CarService _carService;

        public CarsViewModel()
        {
            _carService = new CarService();
            LoadBrandsCommand = new RelayCommand(async () => await LoadBrandsAsync());
            RefreshDataCommand = new RelayCommand(async () => await LoadCarsAsync());
            InitializeAsync(); // Инициализация асинхронных операций
        }

        private async Task InitializeAsync()
        {
            await LoadCarsAsync();
            LoadBrandsCommand.Execute(null);
        }




        private async Task LoadCarsAsync()
        {
            try
            {
                var cars = await Task.Run(() => _carService.GetAllCars());
                Cars = new ObservableCollection<Car>(cars);
                FilteredCars = new ObservableCollection<Car>(Cars);
            }
            catch (Exception ex)
            {
                // Обработка ошибок (например, показ сообщения пользователю)
                Debug.WriteLine("Error loading cars: " + ex.Message);
            }
        }




        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private void FilterCars()
        {
            var filtered = new ObservableCollection<Car>(Cars);
            if (SelectedBrand != null)
            {
                filtered = new ObservableCollection<Car>(filtered.Where(car => car.BrandName == SelectedBrand.BrandName));
            }
            if (SelectedModel != null)
            {
                filtered = new ObservableCollection<Car>(filtered.Where(car => car.Model == SelectedModel.ModelName));
            }

            FilteredCars = filtered; // Обновление отфильтрованного списка
        }


    }
}
