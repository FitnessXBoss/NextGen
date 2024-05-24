using NextGen.src.Data.Database.Models;
using NextGen.src.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NextGen.src.UI.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для CarCatalogModelControl.xaml
    /// </summary>
    public partial class CarCatalogModelControl : UserControl
    {
        public CarCatalogModelControl()
        {
            InitializeComponent();
            // Уберите инициализацию ViewModel здесь
        }

        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            if (parentObject == null) return null;
            if (parentObject is T parent) return parent;
            return FindParent<T>(parentObject);
        }

        private void ListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem is CarWithTrimDetails selectedCar)
            {
                var detailsControl = new CarDetailsControl(selectedCar.CarId);
                detailsControl.DataContext = new CarDetailsViewModel(selectedCar.CarId);

                var dashboardViewModel = FindParent<DashboardWindow>(this)?.DataContext as DashboardViewModel;
                if (dashboardViewModel != null)
                {
                    dashboardViewModel.OpenCarDetailsControl(detailsControl, selectedCar.ModelName, selectedCar.BrandName, selectedCar.CarId.ToString(), selectedCar.TrimName);
                }
            }
        }






    }
}
