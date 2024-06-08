using NextGen.src.UI.ViewModels;
using System.Windows.Controls;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class MessageDialog : UserControl
    {
        public MessageDialog()
        {
            InitializeComponent();
            DataContext = new MessageDialogViewModel(); // Привязка контекста данных
        }
    }
}
