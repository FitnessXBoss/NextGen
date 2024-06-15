using System.Threading.Tasks;
using System.Windows;

namespace NextGen.src.Services
{
    public interface IPaymentStatusService
    {
        Task NotifyPaymentStatusAsync(PaymentNotification notification);
    }

    public class PaymentStatusService : IPaymentStatusService
    {
        public Task NotifyPaymentStatusAsync(PaymentNotification notification)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"Payment received successfully!\nComment: {notification.Comment}\nAmount: {notification.Amount}\nSender: {notification.Sender}", "Payment Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            });

            return Task.CompletedTask;
        }
    }

    public class PaymentNotification
    {
        public string Comment { get; set; }
        public string Amount { get; set; }
        public string Sender { get; set; }
    }
}
