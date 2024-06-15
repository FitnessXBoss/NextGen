using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using NextGen.src.Services.Api;
using NextGen.src.Services.Document;
using NextGen.src.Services.Security;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Models;    
using NextGen.src.UI.Views;
using NextGen.src.UI.Views.UserControls;
using System.Windows.Media.Imaging;
using QRCoder;
using static NextGen.src.Services.Api.PaymentProcessor;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Text.Json;
using System.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace NextGen.src.UI.ViewModels
{
    public class SalesContractViewModel : INotifyPropertyChanged
    {
        private readonly OrganizationService _organizationService;
        private readonly CarService _carService;
        private readonly DocumentGenerator _documentGenerator;
        private readonly TemplateService _templateService;
        private readonly UserSessionService _userSessionService;
        private readonly PaymentProcessor _paymentProcessor;
        private readonly string destinationFolder;
        private decimal _tonToRubRate;


        public SalesContractViewModel(
            OrganizationService organizationService,
            CarService carService,
            DocumentGenerator documentGenerator,
            TemplateService templateService,
            UserSessionService userSessionService,
            PaymentProcessor paymentProcessor)
        {
            _organizationService = organizationService;
            _carService = carService;
            _documentGenerator = documentGenerator;
            _templateService = templateService;
            _userSessionService = userSessionService;
            _paymentProcessor = paymentProcessor;

            SelectedCustomer = TempDataStore.SelectedCustomer;
            SaveContractCommand = new RelayCommand(SaveContract);
            CloseCommand = new RelayCommand(CloseWindow);
            PayCommand = new RelayCommand(async () => await Pay());
            MissingFields = new ObservableCollection<string>();

            // Определяем путь до папки загрузок текущего пользователя
            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            destinationFolder = Path.Combine(downloadsFolder, "NextGen Group");

            // Создаем папку "NextGen Group" если ее нет
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            _tonToRubRate = _paymentProcessor.GetTonToRubRate();
            IsPayButtonVisible = true;
        }

        public void Initialize(int carId)
        {
            CarId = carId;
            LoadCarDetails(carId);
        }

        public Customer SelectedCustomer { get; }
        public CarDetails Car { get; private set; }

        private int _carId;
        public int CarId
        {
            get => _carId;
            set
            {
                if (_carId != value)
                {
                    _carId = value;
                    OnPropertyChanged();
                    LoadCarDetails(_carId);
                }
            }
        }

        private string _paymentStatus;
        private string _walletAddress;
        private BitmapImage _qrCodeImage;
        private string _errorMessage;
        private bool _isQrCodeVisible;
        private bool _isPayButtonVisible;

        public string PaymentStatus
        {
            get => _paymentStatus;
            set
            {
                _paymentStatus = value;
                OnPropertyChanged(nameof(PaymentStatus));
            }
        }

        public string WalletAddress
        {
            get => _walletAddress;
            set
            {
                _walletAddress = value;
                OnPropertyChanged(nameof(WalletAddress));
            }
        }

        public BitmapImage QrCodeImage
        {
            get => _qrCodeImage;
            set
            {
                _qrCodeImage = value;
                OnPropertyChanged(nameof(QrCodeImage));
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public bool IsQrCodeVisible
        {
            get => _isQrCodeVisible;
            set
            {
                _isQrCodeVisible = value;
                OnPropertyChanged(nameof(IsQrCodeVisible));
            }
        }

        public bool IsPayButtonVisible
        {
            get => _isPayButtonVisible;
            set
            {
                _isPayButtonVisible = value;
                OnPropertyChanged(nameof(IsPayButtonVisible));
            }
        }

        public string CustomerFullName => SelectedCustomer != null ? $"{SelectedCustomer.FirstName} {SelectedCustomer.LastName}" : string.Empty;
        public string CustomerEmail => SelectedCustomer?.Email ?? string.Empty;
        public string CustomerPhone => SelectedCustomer?.Phone ?? string.Empty;

        public DateTime ContractDate { get; set; } = DateTime.Now;
        public string PlaceOfIssue { get; set; }
        public string City { get; set; }
        private static int contractNumberCounter = 1;
        public string ContractNumber => $"A{contractNumberCounter++.ToString("D5")}";

        private ObservableCollection<string> _missingFields;
        public ObservableCollection<string> MissingFields
        {
            get => _missingFields;
            set
            {
                _missingFields = value;
                OnPropertyChanged(nameof(MissingFields));
            }
        }

        public ICommand SaveContractCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand PayCommand { get; }

        private async Task Pay()
        {
            Debug.WriteLine("Начало выполнения Pay()");
            IsQrCodeVisible = false;
            ErrorMessage = string.Empty;
            PaymentStatus = "Идет генерация информации о платеже...";

            decimal amountToPay = 0.5m; // Пример суммы для оплаты в TON
            var paymentInfo = await GeneratePaymentInfo(amountToPay);

            if (!string.IsNullOrEmpty(paymentInfo.tonLink))
            {
                Debug.WriteLine("Генерация и отображение QR-кода");
                GenerateAndDisplayQRCode(paymentInfo.tonLink);
                PaymentStatus = $"Expected Comment: {paymentInfo.uniqueId}\nExpected Amount: {amountToPay} TON (~{amountToPay * _tonToRubRate} рублей)";
                WalletAddress = $"Wallet Address: {paymentInfo.address}";
            }
            else
            {
                Debug.WriteLine("Ошибка: Не удалось сгенерировать информацию о платеже");
                ErrorMessage = "Failed to generate payment info.";
                PaymentStatus = string.Empty;
            }
        }

        public void UpdatePaymentStatus(PaymentNotification notification)
        {
            try
            {
                Debug.WriteLine("Начало выполнения UpdatePaymentStatus()");
                var amountInTON = Convert.ToDecimal(notification.Amount) / 1_000_000_000;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Debug.WriteLine("Обновление UI после успешной оплаты");
                    QrCodeImage = null;
                    IsQrCodeVisible = false;

                    PaymentStatus = $"Payment was successful!\n" +
                                    $"Comment: {notification.Comment}\n" +
                                    $"Amount: {amountInTON} TON (~{amountInTON * _tonToRubRate} рублей)\n" +
                                    $"Sender: {notification.Sender}";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in UpdatePaymentStatus: {ex.Message}");
                throw; // Пробросьте исключение, чтобы его могли поймать на уровне сервиса
            }
        }



        private async Task<(string tonLink, string uniqueId, string address)> GeneratePaymentInfo(decimal amount)
        {
            Debug.WriteLine("Начало выполнения GeneratePaymentInfo()");
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(JsonSerializer.Serialize(new { amount }), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("http://localhost:3001/generate-payment", content);
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"Error: Server responded with status code: {response.StatusCode}");
                        ErrorMessage = "Server error. Please try again later.";
                        return (null, null, null);
                    }

                    var responseBody = await response.Content.ReadAsStringAsync();
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
                        ErrorMessage = "Failed to get payment data.";
                        return (null, null, null);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error generating payment info: {ex.Message}");
                    ErrorMessage = $"Error: {ex.Message}";
                    return (null, null, null);
                }
            }
        }

        private void GenerateAndDisplayQRCode(string tonLink)
        {
            Debug.WriteLine("Начало выполнения GenerateAndDisplayQRCode()");
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(tonLink, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                QrCodeImage = ConvertToBitmapImage(qrCode.GetGraphic(20));
                IsQrCodeVisible = true;
                Debug.WriteLine("QR-код успешно сгенерирован и отображен");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error displaying QR code: {ex.Message}";
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

        public class PaymentInfoResponse
        {
            public string address { get; set; }
            public string uniqueId { get; set; }
            public string amount { get; set; }
            public string tonLink { get; set; }
        }

        public class PaymentNotification
        {
            public string Comment { get; set; }
            public string Amount { get; set; }
            public string Sender { get; set; }
        }
        private void LoadCarDetails(int carId)
        {
            var carDetails = _carService.GetCarDetails(carId);
            if (carDetails != null)
            {
                Car = carDetails;
                Debug.WriteLine($"Car Details Loaded in SalesContractViewModel: CarId={carDetails.CarId}, VIN={carDetails.VIN}, Year={carDetails.Year}, Color={carDetails.Color}, HorsePower={carDetails.HorsePower}, Price={carDetails.Price}, ModelName={carDetails.ModelName}, TrimName={carDetails.TrimName}");
                OnPropertyChanged(nameof(Car));
            }
            else
            {
                ShowCustomMessageBox("Не удалось загрузить данные автомобиля.", "Ошибка", CustomMessageBox.MessageKind.Error, false, "ОК");
            }
        }

        private async void SaveContract()
        {
            var organization = _organizationService.GetOrganization();

            if (organization == null)
            {
                await ShowCustomMessageBox("Информация об организации не найдена.", "Ошибка", CustomMessageBox.MessageKind.Error, false, "ОК");
                return;
            }

            if (Car == null || SelectedCustomer == null)
            {
                await ShowCustomMessageBox("Не удалось загрузить данные автомобиля или клиента.", "Ошибка", CustomMessageBox.MessageKind.Error, false, "ОК");
                return;
            }

            if (!await _userSessionService.LoadAdditionalUserDataAsync())
            {
                await ShowCustomMessageBox("Не удалось загрузить данные текущего пользователя.", "Ошибка", CustomMessageBox.MessageKind.Error, false, "ОК");
                return;
            }

            var currentUser = _userSessionService.CurrentUser;
            if (currentUser == null)
            {
                await ShowCustomMessageBox("Не удалось определить текущего пользователя.", "Ошибка", CustomMessageBox.MessageKind.Error, false, "ОК");
                return;
            }

            string priceInWords = NumberToWordsConverter.Convert(Car.Price);
            string employeeFullName = currentUser.FullName;
            string employeeNameInInitials = ConvertToInitials(employeeFullName);
            string customerFullName = SelectedCustomer.FullName;
            string customerNameInInitials = ConvertToInitials(customerFullName);

            var placeholders = new Dictionary<string, string>
            {
                { "{{Название компании}}", organization.Name ?? string.Empty },
                { "{{Адрес компании}}", organization.Address ?? string.Empty },
                { "{{Телефон компании}}", organization.Phone ?? string.Empty },
                { "{{Email компании}}", organization.Email ?? string.Empty },
                { "{{Сайт компании}}", organization.Website ?? string.Empty },
                { "{{Регистрационный номер компании}}", organization.RegistrationNumber ?? string.Empty },
                { "{{ИНН компании}}", organization.INN ?? string.Empty },
                { "{{КПП компании}}", organization.KPP ?? string.Empty },
                { "{{ОКПО компании}}", organization.OKPO ?? string.Empty },
                { "{{Рассчетный счет компании}}", organization.BankAccount ?? string.Empty },
                { "{{Корреспондентский счет компании}}", organization.CorrespondentAccount ?? string.Empty },
                { "{{Банк компании}}", organization.BankName ?? string.Empty },
                { "{{БИК компании}}", organization.BIK ?? string.Empty },

                { "{{ФИО директора}}", organization.DirectorFullName ?? string.Empty },
                { "{{Должность директора}}", organization.DirectorTitle ?? string.Empty },
                { "{{Доверенность}}", organization.PowerOfAttorney ?? string.Empty },
                { "{{Город}}", organization.City ?? string.Empty },

                { "{{ФИО сотрудника}}", employeeFullName ?? string.Empty },
                { "{{Должность сотрудника}}", "Старший менеджер отдела продаж" },
                { "{{ФИО сотрудника в инициалах}}", employeeNameInInitials ?? string.Empty },

                { "{{ФИО покупателя}}", customerFullName ?? string.Empty },
                { "{{ФИО покупателя в инициалах}}", customerNameInInitials ?? string.Empty },
                { "{{Номер паспорта покупателя}}", SelectedCustomer?.PassportNumber ?? string.Empty },
                { "{{Дата выдачи паспорта}}", SelectedCustomer?.PassportIssueDate.ToString("dd.MM.yyyy") ?? string.Empty },
                { "{{Кем выдан паспорт}}", SelectedCustomer?.PassportIssuer ?? string.Empty },
                { "{{Адрес покупателя}}", SelectedCustomer?.Address ?? string.Empty },
                { "{{Телефон покупателя}}", CustomerPhone ?? string.Empty },

                { "{{Тип ТС}}", Car.CarBodyType ?? string.Empty },
                { "{{Код модели}}", Car.TrimName ?? string.Empty },
                { "{{Модель ТС}}", Car.ModelName ?? string.Empty },
                { "{{Год выпуска ТС}}", Car.Year.ToString() ?? string.Empty },
                { "{{Цвет ТС}}", Car.Color ?? string.Empty },
                { "{{Мощность двигателя}}", Car.HorsePower ?? string.Empty },
                { "{{VIN}}", Car.VIN ?? string.Empty },
                { "{{Цена}}", Car.Price.ToString("C") ?? string.Empty },
                { "{{Цена прописью}}", priceInWords },

                { "{{Дата составления}}", ContractDate.ToString("dd.MM.yyyy") ?? string.Empty },
                { "{{Номер договора}}", ContractNumber ?? string.Empty },
                { "{{Дата договора}}", DateTime.Now.ToString("dd.MM.yyyy") ?? string.Empty }
            };

            try
            {
                // Создаем временные файлы
                string tempFolder = Path.GetTempPath();
                string tempContractPath = Path.Combine(tempFolder, "Предварительный договор купли-продажи_temp.docx");
                string tempActPath = Path.Combine(tempFolder, "Акт приема-передачи_temp.docx");
                string tempSaleContractPath = Path.Combine(tempFolder, "Договор купли-продажи_temp.docx");

                // Загружаем и сохраняем шаблоны во временные файлы
                SaveTemplateToFile("Предварительный договор купли-продажи", tempContractPath);
                SaveTemplateToFile("Акт приема-передачи", tempActPath);
                SaveTemplateToFile("Договор купли-продажи", tempSaleContractPath);

                // Проверка существования файлов
                Debug.WriteLine($"Checking if temporary files exist:");
                Debug.WriteLine($"Contract path: {tempContractPath}, Exists: {File.Exists(tempContractPath)}");
                Debug.WriteLine($"Act path: {tempActPath}, Exists: {File.Exists(tempActPath)}");
                Debug.WriteLine($"Sale contract path: {tempSaleContractPath}, Exists: {File.Exists(tempSaleContractPath)}");

                // Проверка и ожидание готовности файлов
                WaitForFileAvailability(tempContractPath);
                WaitForFileAvailability(tempActPath);
                WaitForFileAvailability(tempSaleContractPath);

                // Создаем папку для клиента
                string customerFolderName = $"{SelectedCustomer.Id}_{SelectedCustomer.LastName}_{SelectedCustomer.FirstName}_{SelectedCustomer.MiddleName}";
                string customerFolderPath = Path.Combine(destinationFolder, customerFolderName);
                Directory.CreateDirectory(customerFolderPath);

                // Теперь генерация документов
                GenerateAndValidateDocument("Предварительный договор купли-продажи", tempContractPath, placeholders);
                GenerateAndValidateDocument("Акт приема-передачи", tempActPath, placeholders);
                GenerateAndValidateDocument("Договор купли-продажи", tempSaleContractPath, placeholders);

                // Перемещаем документы в папку клиента и переименовываем их
                string renamedContractPath = await MoveAndRenameDocument(tempContractPath, customerFolderPath, "Предварительный договор купли-продажи");
                string renamedActPath = await MoveAndRenameDocument(tempActPath, customerFolderPath, "Акт приема-передачи");
                string renamedSaleContractPath = await MoveAndRenameDocument(tempSaleContractPath, customerFolderPath, "Договор купли-продажи");

                // Удаление временных файлов
                DeleteTemporaryFile(tempContractPath);
                DeleteTemporaryFile(tempActPath);
                DeleteTemporaryFile(tempSaleContractPath);

                // Открываем проводник с папкой клиента
                OpenFolder(customerFolderPath);

                var changedFiles = new List<string>();
                if (renamedContractPath != null) changedFiles.Add("Предварительный договор купли-продажи");
                if (renamedActPath != null) changedFiles.Add("Акт приема-передачи");
                if (renamedSaleContractPath != null) changedFiles.Add("Договор купли-продажи");

                if (changedFiles.Count > 0)
                {
                    await ShowCustomMessageBox($"Документы для клиента {customerFullName} сохранены.\nИзмененные файлы: {string.Join(", ", changedFiles)}", "Успех", CustomMessageBox.MessageKind.Success);
                }
                else
                {
                    await ShowCustomMessageBox($"Документы для клиента {customerFullName} сохранены.", "Успех", CustomMessageBox.MessageKind.Success);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during SaveContract: {ex.Message}\nStack Trace: {ex.StackTrace}");
                await ShowCustomMessageBox($"Произошла ошибка при создании документа: {ex.Message}\nStack Trace: {ex.StackTrace}", "Ошибка", CustomMessageBox.MessageKind.Error, false, "ОК");
            }
        }

        private string ConvertToInitials(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;

            var parts = fullName.Split(' ');
            if (parts.Length == 2) return $"{parts[0]} {parts[1][0]}.";
            if (parts.Length == 3) return $"{parts[0]} {parts[1][0]}. {parts[2][0]}.";
            return fullName;
        }

        private void SaveTemplateToFile(string templateName, string filePath)
        {
            try
            {
                byte[] templateContent = _templateService.GetTemplate(templateName);
                if (templateContent == null || templateContent.Length == 0)
                {
                    throw new Exception($"Template content for '{templateName}' is empty or not found.");
                }
                File.WriteAllBytes(filePath, templateContent);
                Debug.WriteLine($"Template {templateName} saved to {filePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving template {templateName} to {filePath}: {ex.Message}");
                throw;
            }
        }

        private bool IsFileReady(string filename)
        {
            try
            {
                using (var inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return inputStream.Length > 0;
                }
            }
            catch (IOException ex)
            {
                // Файл занят, ещё не доступен для использования
                Debug.WriteLine($"File {filename} is not ready: {ex.Message}");
                return false;
            }
        }

        private void WaitForFileAvailability(string filePath)
        {
            int attempts = 0;
            while (!IsFileReady(filePath) && attempts < 10) // Проверяем до 10 раз
            {
                System.Threading.Thread.Sleep(500); // Подождите 500 мс
                attempts++;
            }

            if (attempts >= 10)
            {
                throw new Exception($"Файл не стал доступен в течение ожидания: {filePath}");
            }
        }

        private void GenerateAndValidateDocument(string templateName, string filePath, Dictionary<string, string> placeholders)
        {
            try
            {
                var emptyFields = _documentGenerator.GenerateDocument(templateName, filePath, placeholders);
                if (emptyFields.Count > 0)
                {
                    UpdateMissingFields(emptyFields);
                    throw new Exception($"Не заполнены следующие поля: {string.Join(", ", emptyFields)}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during GenerateAndValidateDocument for {templateName}: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw;
            }
        }

        private void UpdateMissingFields(List<string> emptyFields)
        {
            MissingFields.Clear();
            foreach (var field in emptyFields)
            {
                MissingFields.Add(field);
            }
        }

        private async Task<string> MoveAndRenameDocument(string filePath, string destinationFolder, string baseFileName)
        {
            string fileName = Path.GetFileName(filePath);
            string customerName = $"{SelectedCustomer.LastName} {SelectedCustomer.FirstName} {SelectedCustomer.MiddleName}";
            string date = DateTime.Now.ToString("yyyy-MM-dd");
            string newFileName = $"{baseFileName} {SelectedCustomer.Id} {customerName} ({date}).docx";
            string newFilePath = Path.Combine(destinationFolder, newFileName);

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            if (File.Exists(newFilePath))
            {
                DateTime creationTime = File.GetCreationTime(newFilePath);
                string message = $"Файл '{newFileName}' уже существует, создан {creationTime}. Заменить его?";
                var result = await ShowCustomMessageBox(message, "Предупреждение", CustomMessageBox.MessageKind.Warning, true, "Заменить", "Отмена");
                if (!result)
                {
                    return null; // Возвращаем null, если файл не был заменен
                }

                File.Delete(newFilePath);
            }

            File.Move(filePath, newFilePath);

            return newFilePath;
        }

        private void OpenFolder(string folderPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = folderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private async Task<bool> ShowCustomMessageBox(string message, string title = "Уведомление", CustomMessageBox.MessageKind kind = CustomMessageBox.MessageKind.Notification, bool showSecondaryButton = false, string primaryButtonText = "ОК", string secondaryButtonText = "Отмена")
        {
            var view = new CustomMessageBox(message, title, kind, showSecondaryButton, primaryButtonText, secondaryButtonText);
            var result = await DialogHost.Show(view, "RootDialogHost");
            return result is bool boolean && boolean;
        }

        private void DeleteTemporaryFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.WriteLine($"Deleted temporary file: {filePath}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting temporary file {filePath}: {ex.Message}");
            }
        }

        private void CloseWindow()
        {
            var dashboardViewModel = Application.Current.Windows.OfType<DashboardWindow>().FirstOrDefault()?.DataContext as DashboardViewModel;
            if (dashboardViewModel != null)
            {
                dashboardViewModel.CurrentContent = null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
