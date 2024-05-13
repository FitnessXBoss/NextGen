using NextGen.src.Data.Database.Models;
using NextGen.src.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NextGen.src.UI.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для CarsControl.xaml
    /// </summary>
    public partial class CarsControl : UserControl
    {
        public CarsControl()
        {
            InitializeComponent();
            DataContext = new CarsViewModel();
        }


        public static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            // Получаем родителя элемента
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            // Если достигнут корень, возвращаем null
            if (parentObject == null) return null;

            // Если родитель имеет нужный тип, возвращаем его
            if (parentObject is T parent) return parent;

            // Иначе продолжаем подниматься по дереву
            return FindParent<T>(parentObject);
        }

        private void ListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox != null && listBox.SelectedItem is Car selectedCar)
            {
                var editor = new CarEditorControl();
                editor.DataContext = selectedCar;

                var dashboardViewModel = FindParent<DashboardWindow>(this)?.DataContext as DashboardViewModel;
                if (dashboardViewModel != null)
                {
                    dashboardViewModel.OpenCarUserControl(editor, selectedCar.Model, selectedCar.BrandName, selectedCar.ModelId.ToString());
                }
            }
        }

    }
}
