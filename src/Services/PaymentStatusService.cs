using NextGen.src.Models;
using NextGen.src.UI.ViewModels;
using System;
using System.Diagnostics;

namespace NextGen.src.Services
{
    public class PaymentStatusService : IPaymentStatusService
    {
        private readonly Func<int, SalesContractViewModel> _viewModelFactory;

        public PaymentStatusService(Func<int, SalesContractViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void UpdatePaymentStatus(NextGen.src.Models.PaymentNotification notification)
        {
            try
            {
                int carId = 1; // Замените на реальный carId

                var viewModel = _viewModelFactory(carId);
                viewModel.UpdatePaymentStatus(new SalesContractViewModel.PaymentNotification
                {
                    Comment = notification.Comment,
                    Amount = notification.Amount,
                    Sender = notification.Sender
                });

                Debug.WriteLine($"Updating payment status for Sender: {notification.Sender}");
                Debug.WriteLine($"Amount: {notification.Amount}");
                Debug.WriteLine($"Comment: {notification.Comment}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating payment status: {ex.Message}");
                throw; // Пробросьте исключение, чтобы его могли поймать на уровне контроллера
            }
        }
    }
}
