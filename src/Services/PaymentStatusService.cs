using NextGen.src.Models;
using NextGen.src.UI.ViewModels;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace NextGen.src.Services
{
    public class PaymentStatusService : IPaymentStatusService
    {
        private readonly Func<int, SalesContractViewModel> _viewModelFactory;
        private readonly PaymentSettings _paymentSettings;

        public PaymentStatusService(Func<int, SalesContractViewModel> viewModelFactory, PaymentSettings paymentSettings)
        {
            _viewModelFactory = viewModelFactory;
            _paymentSettings = paymentSettings;
        }

        public async Task UpdatePaymentStatusAsync(PaymentNotification notification)
        {
            try
            {
                Debug.WriteLine("Начало выполнения UpdatePaymentStatus()");
                var amountInTON = Convert.ToDecimal(notification.Amount) / 1_000_000_000;

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Debug.WriteLine("Обновление UI после успешной оплаты");
                    var viewModel = _viewModelFactory(notification.CarId); // Assuming you have a carId in the notification
                    viewModel.QrCodeImage = null;
                    viewModel.IsQrCodeVisible = false;
                    viewModel.IsPayButtonVisible = false; // Скрываем кнопку оплаты

                    viewModel.PaymentStatus = $"Payment was successful!\n" +
                                              $"Comment: {notification.Comment}\n" +
                                              $"Amount: {amountInTON} TON (~{amountInTON * _paymentSettings.TonToRubRate} рублей)\n" +
                                              $"Sender: {notification.Sender}";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdatePaymentStatus: {ex.Message}");
                throw; // Пробросьте исключение, чтобы его могли поймать на уровне сервиса
            }
        }
    }
}
