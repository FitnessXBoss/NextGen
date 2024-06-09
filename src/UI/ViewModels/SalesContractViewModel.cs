using System.Windows;
using System.Windows.Input;
using NextGen.src.Data.Database.Models;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Views;

namespace NextGen.src.UI.ViewModels
{
    public class SalesContractViewModel : BaseViewModel
    {
        public Customer SelectedCustomer { get; }

        public string CustomerFullName => $"{SelectedCustomer.FirstName} {SelectedCustomer.LastName}";
        public string CustomerEmail => SelectedCustomer.Email;
        public string CustomerPhone => SelectedCustomer.Phone;

        public ICommand SaveContractCommand { get; }
        public ICommand CloseCommand { get; }

        public SalesContractViewModel(Customer selectedCustomer)
        {
            SelectedCustomer = selectedCustomer;
            SaveContractCommand = new RelayCommand(SaveContract);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void SaveContract()
        {
            MessageBox.Show("Договор сохранен.");
        }

        private void CloseWindow()
        {
            var dashboardViewModel = Application.Current.Windows.OfType<DashboardWindow>().FirstOrDefault()?.DataContext as DashboardViewModel;
            if (dashboardViewModel != null)
            {
                dashboardViewModel.CurrentContent = null;
            }
        }
    }
}
