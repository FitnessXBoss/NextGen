using NextGen.src.UI.ViewModels;
using System.Windows.Controls;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class AddCustomerDialog : UserControl
    {
        public AddCustomerDialog()
        {
            InitializeComponent();
        }

        private void EmailComboBox_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var viewModel = DataContext as AddCustomerDialogViewModel;
            if (viewModel != null && sender is ComboBox comboBox)
            {
                viewModel.SetEmailComboBox(comboBox);
            }
        }
    }
}
