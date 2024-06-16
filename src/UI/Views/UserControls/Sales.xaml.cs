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
using System.Diagnostics;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.Models;

namespace NextGen.src.UI.Views.UserControls
{
    public partial class Sales : UserControl
    {
        private const string CoinGeckoApiKey = "CG-yYqDknPmdtmAjMbuKhnP6y13";
        private decimal _tonToRubRate = 0m;
        private decimal _amountToPay = 0.5m;

        public Sales()
        {
            InitializeComponent();
            LoadTonToRubRate();
        }

        private async void Payment(object sender, RoutedEventArgs e)
        {
            PaymentButton.IsEnabled = false;

            var paymentInfo = await GeneratePaymentInfo(_amountToPay);

            if (!string.IsNullOrEmpty(paymentInfo.tonLink))
            {
                GenerateAndDisplayQRCode(paymentInfo.tonLink);
                PaymentStatusText.Text = $"Ожидаемый комментарий: {paymentInfo.uniqueId}\nОжидаемая сумма: {_amountToPay} TON";
                WalletAddressText.Text = $"Адрес кошелька: {paymentInfo.address}";
                ErrorMessageText.Text = "";
                await ListenForPaymentSuccess(paymentInfo.uniqueId, paymentInfo.address, _amountToPay);
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

        private async Task ListenForPaymentSuccess(string uniqueId, string walletAddress, decimal amountToPay)
        {
            using (HttpClient client = new HttpClient())
            {
                while (true)
                {
                    try
                    {
                        var response = await client.GetAsync($"http://localhost:5220/api/payment/checkStatus?uniqueId={uniqueId}");
                        if (response.IsSuccessStatusCode)
                        {
                            var responseBody = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine($"Response from server: {responseBody}");
                            var paymentStatus = JsonSerializer.Deserialize<PaymentStatusResponse>(responseBody);

                            if (paymentStatus != null)
                            {
                                Debug.WriteLine($"Parsed PaymentStatus: paymentReceived={paymentStatus.paymentReceived}, sender={paymentStatus.sender}");

                                if (paymentStatus.paymentReceived)
                                {
                                    var amountInRub = amountToPay * _tonToRubRate;
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        PaymentStatusText.Text = "Оплата прошла успешно!";
                                        QrCodeImage.Visibility = Visibility.Collapsed;
                                        PaymentButton.Visibility = Visibility.Collapsed;
                                        ReceiptPanel.Visibility = Visibility.Visible;
                                        ReceiptAddressText.Text = $"Адрес: {walletAddress}";
                                        ReceiptSenderText.Text = $"Отправитель: {paymentStatus.sender}";
                                        ReceiptAmountText.Text = $"Сумма: {amountToPay} TON";
                                        ReceiptAmountRubText.Text = $"Сумма в рублях: {amountInRub} рублей";
                                    });

                                    // Returning payment result
                                    var paymentResult = new PaymentResult
                                    {
                                        Amount = amountToPay,
                                        AmountInRub = amountInRub,
                                        Sender = paymentStatus.sender,
                                        TonToRubRate = _tonToRubRate
                                    };

                                    DialogHost.CloseDialogCommand.Execute(paymentResult, this);

                                    break;
                                }
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

        private async void LoadTonToRubRate()
        {
            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.coingecko.com/api/v3/simple/price?ids=the-open-network&vs_currencies=rub"),
                    Headers =
                    {
                        { "accept", "application/json" },
                        { "x-cg-demo-api-key", CoinGeckoApiKey },
                    },
                };
                try
                {
                    using (var response = await client.SendAsync(request))
                    {
                        response.EnsureSuccessStatusCode();
                        var body = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(body))
                        {
                            if (doc.RootElement.TryGetProperty("the-open-network", out JsonElement tonElement) &&
                                tonElement.TryGetProperty("rub", out JsonElement rubElement) &&
                                rubElement.TryGetDecimal(out decimal rubValue))
                            {
                                _tonToRubRate = rubValue;
                                AmountText.Text = $"Сумма: 1 TON (~{_tonToRubRate} рублей)";
                            }
                            else
                            {
                                throw new Exception("Invalid rate info received.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessageText.Text = $"Ошибка загрузки курса TON к RUB: {ex.Message}";
                }
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            var paymentResult = new PaymentResult
            {
                Sender = ReceiptSenderText.Text,
                Amount = Convert.ToDecimal(ReceiptAmountText.Text.Replace(" TON", "")),
                AmountInRub = Convert.ToDecimal(ReceiptAmountRubText.Text.Replace(" рублей", "")),
                TonToRubRate = _tonToRubRate
            };

            // Закрытие DialogHost с результатом
            DialogHost.CloseDialogCommand.Execute(paymentResult, this);
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
