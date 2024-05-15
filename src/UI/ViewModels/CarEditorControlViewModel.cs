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

        public CarEditorControlViewModel(int modelId)
        {
            _carService = new CarService();
            LoadData(modelId);
        }

        public CarEditorControlViewModel() : this(0) { }

        private async void LoadData(int modelId)
        {
            if (modelId == 0) return;

            var cars = await Task.Run(() =>
            {
                var trims = _carService.GetTrimsByModel(modelId).ToList();
                var trimIds = trims.Select(t => t.TrimId).ToList();
                return _carService.GetCarsByTrims(trimIds);
            });

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
