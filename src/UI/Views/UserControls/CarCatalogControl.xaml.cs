using NextGen.src.Data.Database.Models;
using NextGen.src.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class CarCatalogControl : UserControl
    {
        public CarCatalogControl()
        {
            InitializeComponent();
            DataContext = new CarsViewModel();
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
            if (listBox != null && listBox.SelectedItem is CarSummary selectedCarSummary)
            {
                var editor = new CarCatalogModelControl();
                editor.DataContext = new ViewModels.CarCatalogModelControl(selectedCarSummary.ModelId);

                var dashboardViewModel = FindParent<DashboardWindow>(this)?.DataContext as DashboardViewModel;
                if (dashboardViewModel != null)
                {
                    dashboardViewModel.OpenCarUserControl(editor, selectedCarSummary.ModelName, selectedCarSummary.BrandName, selectedCarSummary.ModelId.ToString());
                }
            }
        }


    }
}
