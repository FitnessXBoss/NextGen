using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGen.src.UI.ViewModels
{
    public class CarDetailsViewModel
    {
        public ObservableCollection<CarImage> CarImages { get; set; }
        private CarService _carService = new CarService();
        private int _carId;

        public CarDetailsViewModel(int carId)
        {
            _carId = carId;
            LoadDetailsForCar(_carId);
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
    }

}
