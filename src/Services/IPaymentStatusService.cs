using NextGen.src.Models;
using System.Threading.Tasks;

namespace NextGen.src.Services
{
    public interface IPaymentStatusService
    {
        Task UpdatePaymentStatusAsync(PaymentNotification notification);
    }
}
