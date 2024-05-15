using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
        public ObservableCollection<Trim> Trims { get; set; } = new ObservableCollection<Trim>();

        private Brand _selectedBrand = null!;
        public Brand? SelectedBrand
        {
            get => _selectedBrand;
            set
            {
                if (_selectedBrand != value)
                {
                    _selectedBrand = value;
                    OnPropertyChanged();
                    LoadModelsForBrand(value?.BrandId ?? 0);
                    FilterCars();
                }
            }
        }

        private Model _selectedModel = null!;
        public Model? SelectedModel
        {
            get => _selectedModel;
            set
            {
                if (_selectedModel != value)
                {
                    _selectedModel = value;
                    OnPropertyChanged();
                    LoadCarDetails(value?.ModelId ?? 0);
                    FilterCars();
                }
            }
        }

        private ObservableCollection<CarSummary> _carSummaries = new ObservableCollection<CarSummary>();
        public ObservableCollection<CarSummary> CarSummaries
        {
            get => _carSummaries;
            set
            {
                _carSummaries = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<CarSummary> _filteredCarSummaries = new ObservableCollection<CarSummary>();
        public ObservableCollection<CarSummary> FilteredCarSummaries
        {
            get => _filteredCarSummaries;
            set
            {
                _filteredCarSummaries = value;
                OnPropertyChanged();
            }
        }

        private readonly CarService _carService;

        public CarsViewModel()
        {
            _carService = new CarService();
            LoadBrandsCommand = new RelayCommand(async () => await LoadBrandsAsync());
            RefreshDataCommand = new RelayCommand(async () => await RefreshDataAsync());
            _ = InitializeAsync(); // Инициализация асинхронных операций
        }

        private async Task RefreshDataAsync()
        {
            await LoadCarSummariesAsync();
            ResetSelections();
        }

        private void ResetSelections()
        {
            SelectedBrand = null;
            SelectedModel = null;
            Models.Clear();
        }

        private async Task InitializeAsync()
        {
            await LoadCarSummariesAsync();
            LoadBrandsCommand.Execute(null);
        }

        private async Task LoadCarSummariesAsync()
        {
            try
            {
                var carSummaries = await Task.Run(() => _carService.GetAllCarSummaries());
                CarSummaries = new ObservableCollection<CarSummary>(carSummaries);
                FilterCars();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading car summaries: " + ex.Message);
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

        public async void LoadCarDetails(int modelId)
        {
            try
            {
                var trims = await Task.Run(() => _carService.GetTrimsByModel(modelId));
                Trims.Clear();
                foreach (var trim in trims)
                {
                    Trims.Add(trim);
                }

                var cars = await Task.Run(() => _carService.GetCarsByTrims(Trims.Select(t => t.TrimId).ToList()));
                Cars.Clear();
                foreach (var car in cars)
                {
                    Cars.Add(car);
                }
                FilterCars(); // Перефильтровать автомобили после загрузки деталей
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error loading car details: " + ex.Message);
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void FilterCars()
        {
            var filtered = new ObservableCollection<CarSummary>(CarSummaries);
            if (SelectedBrand != null)
            {
                filtered = new ObservableCollection<CarSummary>(filtered.Where(cs => cs.BrandName == SelectedBrand.BrandName));
            }
            if (SelectedModel != null)
            {
                filtered = new ObservableCollection<CarSummary>(filtered.Where(cs => cs.ModelName == SelectedModel.ModelName));
            }

            FilteredCarSummaries = filtered;
        }
    }
}
