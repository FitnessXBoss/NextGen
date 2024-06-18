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
            // Уведомление об успешной оплате
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
