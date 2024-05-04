// EmployeeControl.xaml.cs
using NextGen.src.Services;
using NextGen.src.UI.ViewModels;
using System.Windows.Controls;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class EmployeeControl : UserControl
    {
        public EmployeeControl()
        {
            InitializeComponent();
            var databaseService = new DatabaseService(); // Создание сервиса для работы с базой данных
            DataContext = new EmployeeViewModel(databaseService); // Передача сервиса в ViewModel
        }

        private void AddButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Implementation to add a new employee
            // You might want to open a dialog here to input new employee details
        }

        private void EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Implementation to edit an existing employee
            // Typically, you'd want to check if an employee is selected in the grid and open a dialog to edit their details
        }

        private void DeleteButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Implementation to delete an employee
            // Check if an employee is selected and confirm deletion before removing them
        }
    }
}
