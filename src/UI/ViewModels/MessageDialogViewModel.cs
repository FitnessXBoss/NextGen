using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.Helpers;

namespace NextGen.src.UI.ViewModels
{
    public class MessageDialogViewModel : BaseViewModel
    {
        public string Message { get; set; }
        public ICommand CloseCommand { get; }

        public MessageDialogViewModel()
        {
            CloseCommand = new RelayCommand(CloseDialog);
        }

        private void CloseDialog()
        {
            DialogHost.CloseDialogCommand.Execute(null, null);
        }
    }
}
