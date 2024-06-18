using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using NextGen.src.Services.Security;
using OtpNet;
using QRCoder;

namespace NextGen.src.UI.ViewModels
{
    public class SettingsControlViewModel : INotifyPropertyChanged
    {
        private string? _username;
        private string? _twoFactorCode;
        private BitmapImage? _qrCodeImage;
        private string? _temporarySecretKey;

        public string? Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        public string? TwoFactorCode
        {
            get => _twoFactorCode;
            set
            {
                _twoFactorCode = value;
                OnPropertyChanged(nameof(TwoFactorCode));
            }
        }

        public BitmapImage? QrCodeImage
        {
            get => _qrCodeImage;
            set
            {
                _qrCodeImage = value;
                OnPropertyChanged(nameof(QrCodeImage));
            }
        }

        public ICommand SetupGoogleAuthCommand { get; }
        public ICommand ResetQRCodeCommand { get; }
        public ICommand VerifyCodeCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public SettingsControlViewModel()
        {
            SetupGoogleAuthCommand = new RelayCommand(async () => await SetupGoogleAuthAsync());
            ResetQRCodeCommand = new RelayCommand(ResetQRCode);
            VerifyCodeCommand = new RelayCommand(async () => await VerifyTwoFactorCodeAsync());
        }

        private async Task SetupGoogleAuthAsync()
        {
            // Получение текущего пользователя
            var currentUser = UserSessionService.Instance.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            Username = currentUser.Username;

            // Генерация нового секретного ключа
            var key = KeyGeneration.GenerateRandomKey(20);
            _temporarySecretKey = Base32Encoding.ToString(key);
            var userEmail = currentUser.Email; // Используем email текущего пользователя
            var uri = GenerateQRCodeUri(_temporarySecretKey, userEmail, Username);

            // Генерация QR-кода
            QrCodeImage = GenerateQRCode(uri);

            // Сохранение секретного ключа в базе данных
            var authService = new AuthService();
            await authService.SaveSecretKeyAsync(Username, _temporarySecretKey);
        }

        private string GenerateQRCodeUri(string secretKey, string userEmail, string username)
        {
            var otpType = "totp";
            var issuer = "NextGenCompany"; // Используйте название вашего приложения или компании
            var label = $"{username} | {userEmail}";
            var parameters = $"secret={secretKey}&issuer={Uri.EscapeDataString(issuer)}";
            var uri = $"otpauth://{otpType}/{issuer}:{label}?{parameters}";
            return uri;
        }

        private BitmapImage GenerateQRCode(string uri)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(40);
            return LoadBitmap(qrCodeImage);
        }

        private BitmapImage LoadBitmap(System.Drawing.Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        private async Task VerifyTwoFactorCodeAsync()
        {
            var currentUser = UserSessionService.Instance.CurrentUser;
            if (currentUser == null)
            {
                MessageBox.Show("Пожалуйста, войдите в систему.");
                return;
            }

            var authService = new AuthService();
            var isValid = await authService.ValidateTwoFactorCodeAsync(currentUser.UserId, TwoFactorCode);
            if (isValid.Item1)
            {
                MessageBox.Show("Код подтвержден!");
            }
            else
            {
                MessageBox.Show(isValid.Item2 ?? "Неверный код. Пожалуйста, попробуйте еще раз.");
            }
        }

        private void ResetQRCode()
        {
            QrCodeImage = null;
            _temporarySecretKey = null;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
