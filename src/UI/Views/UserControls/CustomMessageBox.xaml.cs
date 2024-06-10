using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class CustomMessageBox : UserControl
    {
        public enum MessageKind
        {
            Notification,
            Error,
            Warning,
            Success
        }

        public CustomMessageBox(string message, string title = "Уведомление", MessageKind kind = MessageKind.Notification, bool showSecondaryButton = false, string primaryButtonText = "ОК", string secondaryButtonText = "Отмена")
        {
            InitializeComponent();
            TitleTextBlock.Text = title;
            MessageTextBlock.Text = message;
            PrimaryButton.Content = primaryButtonText;

            if (showSecondaryButton)
            {
                SecondaryButton.Content = secondaryButtonText;
                SecondaryButton.Visibility = System.Windows.Visibility.Visible;
            }

            switch (kind)
            {
                case MessageKind.Notification:
                    Icon.Kind = PackIconKind.Information;
                    break;
                case MessageKind.Error:
                    Icon.Kind = PackIconKind.Error;
                    break;
                case MessageKind.Warning:
                    Icon.Kind = PackIconKind.Warning;
                    break;
                case MessageKind.Success:
                    Icon.Kind = PackIconKind.CheckCircle;
                    break;
            }
        }
    }
}
