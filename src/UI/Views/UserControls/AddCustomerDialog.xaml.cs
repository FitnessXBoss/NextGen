using NextGen.src.UI.ViewModels;
using System.Windows.Controls;
using System.Windows;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class AddCustomerDialog : UserControl
    {
        public AddCustomerDialog()
        {
            InitializeComponent();
        }

        private void EmailComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as AddCustomerDialogViewModel;
            if (viewModel != null && sender is ComboBox comboBox)
            {
                viewModel.SetEmailComboBox(comboBox);
            }
        }

        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as AddCustomerDialogViewModel;
            if (viewModel != null)
            {
                viewModel.IsDropDownOpen = false;
            }
        }

        private void AddressTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as AddCustomerDialogViewModel;
            if (viewModel != null)
            {
                viewModel.IsAddressDropDownOpen = false;
            }
        }
    }
}
