using System.Collections.ObjectModel;
using System.ComponentModel;  // Для INotifyPropertyChanged
using System.Runtime.CompilerServices; // Для CallerMemberName
using System.Linq; // Для LINQ-запросов

namespace NextGen.src.UI.ViewModels
{
    public class Car
    {
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Year { get; set; }
        public bool IsSold { get; set; }
        public string? BrandIconUrl { get; set; }  // URL для иконки бренда
    }




    public class CarsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        // Метод для вызова события PropertyChanged
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _searchKeyword;
        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword != value)
                {
                    _searchKeyword = value;
                    OnPropertyChanged();
                    FilterCars();
                }
            }
        }

        private ObservableCollection<Car> _filteredCars;
        public ObservableCollection<Car> FilteredCars
        {
            get => _filteredCars;
            set
            {
                _filteredCars = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Car> Cars { get; set; }

        public CarsViewModel()
        {
            Cars = new ObservableCollection<Car>
    {
        new Car {
            Model = "Emperor ETR1",
            ImageUrl = "https://i.postimg.cc/1RMQcN95/A7-18.jpg",
            Price = 1995000,
            Year = 2021,
            IsSold = true,
            Brand = "Emperor",
            BrandIconUrl = "https://i.pinimg.com/originals/6a/75/66/6a7566b6e66d3bd5400892c2983cf1ef.png"
        },

        new Car {
            Model = "Caldina",
            ImageUrl = "https://a.d-cd.net/xZw78mrJ-pAVd4i2f6BRpJYYgh0-960.jpg",
            Price = 1995000,
            Year = 2021,
            IsSold = false,
            Brand = "Toyota",
            BrandIconUrl = "https://content.presspage.com/clients/o_1523.png"
        },

        new Car {
            Model = "Emperor ETR1",
            ImageUrl = "https://tradeins.space/uploads/photo/6817659/2af30c07b58aa0fe1ac38944065709c8.png",
            Price = 1995000,
            Year = 2021,
            IsSold = false,
            Brand = "Emperor",
            BrandIconUrl = "https://i.pinimg.com/originals/6a/75/66/6a7566b6e66d3bd5400892c2983cf1ef.png"
        },

        new Car {
            Model = "Emperor ETR1",
            ImageUrl = "https://i.postimg.cc/1RMQcN95/A7-18.jpg",
            Price = 1995000,
            Year = 2021,
            IsSold = false,
            Brand = "Emperor",
            BrandIconUrl = "https://i.pinimg.com/originals/6a/75/66/6a7566b6e66d3bd5400892c2983cf1ef.png"
        },
         new Car {
            Model = "Emperor ETR1",
            ImageUrl = "https://i.postimg.cc/1RMQcN95/A7-18.jpg",
            Price = 1995000,
            Year = 2021,
            IsSold = false,
            Brand = "Emperor",
            BrandIconUrl = "https://i.pinimg.com/originals/6a/75/66/6a7566b6e66d3bd5400892c2983cf1ef.png"
        },

        new Car {
            Model = "Emperor ETR1",
            ImageUrl = "https://i.postimg.cc/1RMQcN95/A7-18.jpg",
            Price = 1995000,
            Year = 2021,
            IsSold = false,
            Brand = "Emperor",
            BrandIconUrl = "https://i.pinimg.com/originals/6a/75/66/6a7566b6e66d3bd5400892c2983cf1ef.png"
        },

        new Car {
            Model = "Emperor ETR1",
            ImageUrl = "https://i.postimg.cc/1RMQcN95/A7-18.jpg",
            Price = 1995000,
            Year = 2021,
            IsSold = false,
            Brand = "Emperor",
            BrandIconUrl = "https://i.pinimg.com/originals/6a/75/66/6a7566b6e66d3bd5400892c2983cf1ef.png"
        },

        new Car {
            Model = "Emperor ETR1",
            ImageUrl = "https://i.postimg.cc/1RMQcN95/A7-18.jpg",
            Price = 1995000,
            Year = 2021,
            IsSold = true,
            Brand = "Emperor",
            BrandIconUrl = "https://i.pinimg.com/originals/6a/75/66/6a7566b6e66d3bd5400892c2983cf1ef.png"
        },
        // Добавьте другие автомобили по аналогии
    };

            // Изначально покажем все автомобили
            FilteredCars = new ObservableCollection<Car>(Cars);
        }

        // Метод для фильтрации автомобилей по ключевому слову
        private void FilterCars()
        {
            if (string.IsNullOrEmpty(SearchKeyword))
            {
                // Показать все автомобили, если нет поискового ключа
                FilteredCars = new ObservableCollection<Car>(Cars);
            }
            else
            {
                // Фильтрация автомобилей по ключевому слову
                FilteredCars = new ObservableCollection<Car>(Cars.Where(car => car.Model.Contains(SearchKeyword, System.StringComparison.OrdinalIgnoreCase)));
            }
        }
    }
}
