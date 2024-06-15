using System.Windows.Controls;
using NextGen.src.Services;
using NextGen.src.Services.Api;
using NextGen.src.Services.Document;
using NextGen.src.UI.ViewModels;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class SalesContractDialog : UserControl
    {
        public SalesContractDialog()
        {
            InitializeComponent(); 
            DataContext = new SalesContractViewModel(
                new OrganizationService(),
                new CarService(),
                new DocumentGenerator(),
                new TemplateService(),
                UserSessionService.Instance,
                PaymentProcessor.Instance
            );
        }
    }
}
