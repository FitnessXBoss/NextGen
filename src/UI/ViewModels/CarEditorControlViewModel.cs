using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace NextGen.src.UI.ViewModels
{
    public class CarEditorControlViewModel : INotifyPropertyChanged
    {
        private readonly CarService _carService;
        public ObservableCollection<CarWithTrimDetails> Cars { get; private set; } = new ObservableCollection<CarWithTrimDetails>();

        private string? _brandName;
        public string? BrandName
        {
            get => _brandName;
            set
            {
                _brandName = value;
                OnPropertyChanged();
            }
        }

        private string? _modelName;
        public string? ModelName
        {
            get => _modelName;
            set
            {
                _modelName = value;
                OnPropertyChanged();
            }
        }

        private string? _brandIconUrl;
        public string? BrandIconUrl
        {
            get => _brandIconUrl;
            set
            {
                _brandIconUrl = value;
                OnPropertyChanged();
            }
        }

        public CarEditorControlViewModel(int modelId)
        {
            _carService = new CarService();
            LoadData(modelId);
        }

        public CarEditorControlViewModel() : this(0) { }

        private async void LoadData(int modelId)
        {
            if (modelId == 0) return;

            var modelTask = Task.Run(() => _carService.GetModelsByBrand(modelId));
            var carTask = Task.Run(() =>
            {
                var trims = _carService.GetTrimsByModel(modelId).ToList();
                var trimIds = trims.Select(t => t.TrimId).ToList();
                return _carService.GetCarsByTrims(trimIds);
            });

            var models = await modelTask;
            Debug.WriteLine($"Models count: {models?.Count() ?? 0}");

            if (models == null || !models.Any())
            {
                Debug.WriteLine($"No models found for modelId: {modelId}");
                return;
            }

            var model = models.FirstOrDefault(m => m.ModelId == modelId);
            Debug.WriteLine($"Model found: {model?.ModelName}");

            if (model == null)
            {
                Debug.WriteLine($"Model not found with modelId: {modelId}");
                return;
            }

            var brand = await Task.Run(() => _carService.GetAllBrands().FirstOrDefault(b => b.BrandId == model.BrandId));
            Debug.WriteLine($"Brand found: {brand?.BrandName}");

            if (brand == null)
            {
                Debug.WriteLine($"Brand not found for brandId: {model.BrandId}");
                return;
            }

            BrandName = brand.BrandName ?? "Default Brand Name";
            BrandIconUrl = brand.BrandIconUrl ?? "https://celes.club/pictures/uploads/posts/2023-06/1686139250_celes-club-p-risunok-mashina-vid-sboku-risunok-vkontakt-1.jpg";
            ModelName = model.ModelName ?? "Default Model Name";

            var cars = await carTask;

            foreach (var car in cars)
            {
                car.ImageUrl = car.ImageUrl ?? "https://celes.club/pictures/uploads/posts/2023-06/1686139250_celes-club-p-risunok-mashina-vid-sboku-risunok-vkontakt-1.jpg";
                Debug.WriteLine($"CarId: {car.CarId}, TrimId: {car.TrimId}, ImageUrl: {car.ImageUrl}, " +
                                $"AdditionalFeatures: {car.AdditionalFeatures}, Status: {car.Status}, Year: {car.Year}, " +
                                $"Color: {car.Color}, TrimName: {car.TrimName}, TrimDetails: {car.TrimDetails}, " +
                                $"Price: {car.Price}, Transmission: {car.Transmission}, Drive: {car.Drive}, " +
                                $"Fuel: {car.Fuel}, EngineVolume: {car.EngineVolume}, HorsePower: {car.HorsePower}");
            }

            Cars = new ObservableCollection<CarWithTrimDetails>(cars);
            OnPropertyChanged(nameof(Cars));
        }








        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
