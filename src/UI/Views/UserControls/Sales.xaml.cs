using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using QRCoder;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class Sales : UserControl
    {
        public Sales()
        {
            InitializeComponent();
        }

        private async void Payment(object sender, RoutedEventArgs e)
        {
            PaymentButton.IsEnabled = false;

            decimal amountToPay = 0.5m;
            var paymentInfo = await GeneratePaymentInfo(amountToPay);

            if (!string.IsNullOrEmpty(paymentInfo.tonLink))
            {
                GenerateAndDisplayQRCode(paymentInfo.tonLink);
                PaymentStatusText.Text = $"Ожидаемый комментарий: {paymentInfo.uniqueId}\nОжидаемая сумма: {amountToPay} TON";
                WalletAddressText.Text = $"Адрес кошелька: {paymentInfo.address}";
                ErrorMessageText.Text = "";
                await ListenForPaymentSuccess(paymentInfo.uniqueId);
            }
            else
            {
                ErrorMessageText.Text = "Не удалось сгенерировать данные для оплаты.";
                PaymentButton.IsEnabled = true;
            }
        }

        private async Task<(string tonLink, string uniqueId, string address)> GeneratePaymentInfo(decimal amount)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(new { amount }), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("http://localhost:3001/generate-payment", content);
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var paymentInfo = JsonSerializer.Deserialize<PaymentInfoResponse>(responseBody);

                    if (paymentInfo != null)
                    {
                        return (paymentInfo.tonLink, paymentInfo.uniqueId, paymentInfo.address);
                    }
                    else
                    {
                        return (null, null, null);
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessageText.Text = $"Ошибка: {ex.Message}";
                    return (null, null, null);
                }
            }
        }

        private void GenerateAndDisplayQRCode(string tonLink)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(tonLink, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                BitmapImage qrCodeImage = ConvertToBitmapImage(qrCode.GetGraphic(20));
                QrCodeImage.Source = qrCodeImage;
                QrCodeImage.Visibility = Visibility.Visible;
                PaymentButton.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                ErrorMessageText.Text = $"Ошибка отображения QR-кода: {ex.Message}";
                PaymentButton.IsEnabled = true;
            }
        }

        private BitmapImage ConvertToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private async Task ListenForPaymentSuccess(string uniqueId)
        {
            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        var response = await client.GetAsync($"http://localhost:3001/checkStatus?uniqueId={uniqueId}");
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            var paymentStatus = JsonSerializer.Deserialize<PaymentStatusResponse>(responseBody);

                            if (paymentStatus != null && paymentStatus.paymentReceived)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    PaymentStatusText.Text = "Оплата прошла успешно!";
                                    QrCodeImage.Visibility = Visibility.Collapsed;
                                    PaymentButton.Visibility = Visibility.Collapsed;
                                    ReceiptPanel.Visibility = Visibility.Visible;
                                    ReceiptCommentText.Text = $"Комментарий: {paymentStatus.comment}";
                                    ReceiptAmountText.Text = $"Сумма: {paymentStatus.amount} TON";
                                    ReceiptSenderText.Text = $"Отправитель: {paymentStatus.sender}";
                                });
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ErrorMessageText.Text = $"Ошибка проверки статуса оплаты: {ex.Message}";
                    }

                    await Task.Delay(2000);
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            PaymentStatusText.Text = "";
            ErrorMessageText.Text = "";
            WalletAddressText.Text = "";
            ReceiptPanel.Visibility = Visibility.Collapsed;
            PaymentButton.IsEnabled = true;
        }
    }

    public class PaymentInfoResponse
    {
        public string address { get; set; }
        public string uniqueId { get; set; }
        public string amount { get; set; }
        public string tonLink { get; set; }
    }

    public class PaymentStatusResponse
    {
        public bool paymentReceived { get; set; }
        public string comment { get; set; }
        public string amount { get; set; }
        public string sender { get; set; }
    }
}
