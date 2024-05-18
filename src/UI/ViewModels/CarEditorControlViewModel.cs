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

        private int? _selectedTrimId;
        public int? SelectedTrimId
        {
            get => _selectedTrimId;
            set
            {
                if (_selectedTrimId != value)
                {
                    _selectedTrimId = value;
                    OnPropertyChanged();
                    LoadCarsByTrim();
                }
            }
        }



        private async void LoadCarsByTrim()
        {
            if (SelectedTrimId.HasValue)
            {
                var cars = await Task.Run(() => _carService.GetCarsByTrimId(SelectedTrimId.Value));
                var carDetails = cars.Select(car => new CarWithTrimDetails
                {
                    CarId = car.CarId,
                    TrimId = car.TrimId,
                    // Дополните другими свойствами, преобразуя Car в CarWithTrimDetails
                });
                Cars = new ObservableCollection<CarWithTrimDetails>(carDetails);
            }
        }

        public CarEditorControlViewModel() : this(0) { }

        private async void LoadData(int modelId)
        {
            Debug.WriteLine($"Loading data for ModelId: {modelId}");
            if (modelId == 0) return;

            var trims = await Task.Run(() => _carService.GetTrimsByModel(modelId).ToList());
            if (!trims.Any())
            {
                Debug.WriteLine("No trims found for modelId: " + modelId);
                return;
            }

            Debug.WriteLine($"Trims found: {trims.Count}");
            var trimIds = trims.Select(t => t.TrimId).ToList();

            var cars = await Task.Run(() => _carService.GetCarsByTrims(trimIds));
            if (!cars.Any())
            {
                Debug.WriteLine("No cars found for the provided trims");
                return;
            }

            Cars = new ObservableCollection<CarWithTrimDetails>(cars);
            if (Cars.Any())
            {
                // Задайте свойства BrandName, ModelName, и BrandIconUrl первого автомобиля в списке
                BrandName = Cars.First().BrandName;
                ModelName = Cars.First().ModelName;
                BrandIconUrl = Cars.First().BrandIconUrl;
            }
            OnPropertyChanged(nameof(Cars));
            OnPropertyChanged(nameof(BrandName));
            OnPropertyChanged(nameof(ModelName));
            OnPropertyChanged(nameof(BrandIconUrl));
        }



        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
