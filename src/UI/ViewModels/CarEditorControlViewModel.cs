using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

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

            var model = (await modelTask).FirstOrDefault(m => m.ModelId == modelId);
            if (model != null)
            {
                var brand = await Task.Run(() => _carService.GetAllBrands().FirstOrDefault(b => b.BrandId == model.BrandId));
                if (brand != null)
                {
                    BrandName = brand.BrandName;
                    BrandIconUrl = brand.BrandIconUrl;
                }
                ModelName = model.ModelName;
            }

            var cars = await carTask;
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
