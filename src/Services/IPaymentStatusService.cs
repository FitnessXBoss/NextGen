using NextGen.src.Models;

namespace NextGen.src.Services
{
    public interface IPaymentStatusService
    {
        void UpdatePaymentStatus(PaymentNotification notification);
    }
}
