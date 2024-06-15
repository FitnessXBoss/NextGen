using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using QRCoder;

namespace NextGen.src.Services.Api
{
    public class PaymentProcessor
    {
        private static PaymentProcessor _instance;
        private decimal _tonToRubRate = 0m;
        private const string CoinGeckoApiKey = "CG-yYqDknPmdtmAjMbuKhnP6y13";
        public static PaymentProcessor Instance => _instance ??= new PaymentProcessor();

        private PaymentProcessor()
        {
            LoadTonToRubRate().ConfigureAwait(false);
        }

        public async Task<(string tonLink, string uniqueId, string address)> GeneratePaymentInfo(decimal amount)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(new { amount }), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("http://localhost:3001/generate-payment", content).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Debug.WriteLine($"Received payment info: {responseBody}");
                    var paymentInfo = JsonSerializer.Deserialize<PaymentInfoResponse>(responseBody);

                    if (paymentInfo != null)
                    {
                        Debug.WriteLine($"Deserialized payment info: Address={paymentInfo.address}, UniqueId={paymentInfo.uniqueId}, Amount={paymentInfo.amount}, TonLink={paymentInfo.tonLink}");
                        return (paymentInfo.tonLink, paymentInfo.uniqueId, paymentInfo.address);
                    }
                    else
                    {
                        Debug.WriteLine("Deserialization returned null.");
                        return (null, null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error generating payment info: {ex.Message}");
                    return (null, null, null);
                }
            }
        }

        public BitmapImage GenerateQRCode(string tonLink)
        {
            try
            {
                Debug.WriteLine($"Generating QR code for link: {tonLink}");
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(tonLink, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                return ConvertToBitmapImage(qrCode.GetGraphic(20));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error displaying QR code: {ex.Message}");
                throw;
            }
        }

        private BitmapImage ConvertToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Error converting bitmap to image: {ex.Message}");
                throw;
            }
        }

        private async Task LoadTonToRubRate()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://api.coingecko.com/api/v3/simple/price?ids=the-open-network&vs_currencies=rub%2Cusd"),
                Headers =
                {
                    { "accept", "application/json" },
                    { "x-cg-demo-api-key", CoinGeckoApiKey },
                },
            };
            try
            {
                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                    var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    Debug.WriteLine($"CoinGecko API Response: {body}");

                    using (JsonDocument doc = JsonDocument.Parse(body))
                    {
                        if (doc.RootElement.TryGetProperty("the-open-network", out JsonElement tonElement) &&
                            tonElement.TryGetProperty("rub", out JsonElement rubElement) &&
                            rubElement.TryGetDecimal(out decimal rubValue))
                        {
                            _tonToRubRate = rubValue;
                            Debug.WriteLine($"Loaded TON to RUB rate: {_tonToRubRate}");
                        }
                        else
                        {
                            Debug.WriteLine("Invalid rate info: could not extract 'rub' value.");
                            throw new Exception("Invalid rate info received.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading TON to RUB rate: {ex.Message}");
            }
        }

        public decimal GetTonToRubRate() => _tonToRubRate;

        public class PaymentInfoResponse
        {
            public string address { get; set; }
            public string uniqueId { get; set; }
            public string amount { get; set; }
            public string tonLink { get; set; }
        }
    }
}
