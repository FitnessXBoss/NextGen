using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NextGen.src.UI.ViewModels;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class SalesContractDialog : UserControl
    {
        public SalesContractDialog()
        {
            InitializeComponent();
            DataContext = App.AppHost.Services.GetService<SalesContractViewModel>();
        }
    }
}
