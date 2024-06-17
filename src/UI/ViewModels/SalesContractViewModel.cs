using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using MaterialDesignThemes.Wpf;
using NextGen.src.Data.Database.Models;
using NextGen.src.Services;
using NextGen.src.Services.Api;
using NextGen.src.Services.Document;
using NextGen.src.Services.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Configuration;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using NextGen.src.UI.Helpers;
using NextGen.src.UI.Models;
using NextGen.src.UI.Views.UserControls;
using NextGen.src.UI.Views;

namespace NextGen.src.UI.ViewModels
{
    public partial class SalesContractViewModel : ObservableObject
    {
        private readonly OrganizationService _organizationService;
        private readonly CarService _carService;
        private readonly DocumentGenerator _documentGenerator;
        private readonly TemplateService _templateService;
        private readonly UserSessionService _userSessionService;
        private readonly PaymentProcessor _paymentProcessor;
        private readonly SaleService _saleService;
        private readonly string destinationFolder;
        private decimal _tonToRubRate;
        private decimal _paymentAmountInRub;
        private string _paymentSender;
        private bool _isPreliminaryContractGenerated;

        public IRelayCommand OpenPaymentDialogCommand { get; }
        public IRelayCommand SaveContractCommand { get; }
        public IRelayCommand CloseCommand { get; }
        public ObservableCollection<string> MissingFields { get; }

        public SalesContractViewModel(
            OrganizationService organizationService,
            CarService carService,
            DocumentGenerator documentGenerator,
            TemplateService templateService,
            UserSessionService userSessionService,
            SaleService saleService)
        {
            _organizationService = organizationService;
            _carService = carService;
            _documentGenerator = documentGenerator;
            _templateService = templateService;
            _userSessionService = userSessionService;
            _saleService = saleService;

            SelectedCustomer = TempDataStore.SelectedCustomer;
            SaveContractCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(async () => await SaveContract());
            CloseCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CloseWindow);
            OpenPaymentDialogCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(async () => await OpenPaymentDialog(), () => _isPreliminaryContractGenerated);
            MissingFields = new ObservableCollection<string>();

            // Определяем путь до папки загрузок текущего пользователя
            string downloadsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            destinationFolder = Path.Combine(downloadsFolder, "NextGen Group");

            // Создаем папку "NextGen Group" если ее нет
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }
        }

        private async Task OpenPaymentDialog()
        {
            // Получаем текущий курс TON к RUB
            await LoadTonToRubRate();

            // Расчитываем стоимость машины в TON
            decimal carPrice = Car.Price; // Цена в рублях
            decimal amountToPayInRubles = carPrice / 10000; // Уменьшаем сумму в 10000 раз (4700000 -> 470)
            decimal amountToPayInTon = amountToPayInRubles / _tonToRubRate;

            var view = new Sales(amountToPayInTon, _tonToRubRate);
            var result = await DialogHost.Show(view, "RootDialogHost");

            if (result is NextGen.src.UI.Views.UserControls.PaymentResult paymentResult)
            {
                // Обработка результата оплаты
                _paymentSender = paymentResult.Sender;
                _paymentAmountInRub = paymentResult.AmountInRub * 10000; // Конвертируем обратно в изначальную сумму (470 -> 4700000)
                _tonToRubRate = paymentResult.TonToRubRate;

                // Логика после успешного платежа
                Debug.WriteLine($"Payment Received: Sender={_paymentSender}, Amount in RUB={_paymentAmountInRub}, Rate={_tonToRubRate}");

                // Сохраняем данные об оплате в базу данных
                _saleService.RecordPayment(Car.CarId, _paymentSender, _paymentAmountInRub);

                // Создаем папку для клиента
                string customerFolderName = $"{SelectedCustomer.Id}_{SelectedCustomer.LastName}_{SelectedCustomer.FirstName}_{SelectedCustomer.MiddleName}";
                string customerFolderPath = Path.Combine(destinationFolder, customerFolderName);
                Directory.CreateDirectory(customerFolderPath);

                // Создаем и сохраняем PDF чек в папку клиента
                string receiptPath = GeneratePdfReceipt(customerFolderPath);

                // Обновляем статус машины и записываем продажу в базу данных
                _saleService.RecordSale(Car.CarId, _userSessionService.CurrentUser.UserId, SelectedCustomer.CustomerId, _paymentAmountInRub);

                // Открываем папку с чеком и другими документами
                OpenFolder(customerFolderPath);

                // Генерируем оставшиеся документы
                GenerateRemainingDocuments(customerFolderPath);

                await ShowCustomMessageBox($"Оплата успешно завершена. Документы сохранены в папке {Path.GetDirectoryName(receiptPath)}", "Успех", CustomMessageBox.MessageKind.Success);
            }
        }

        private async Task LoadTonToRubRate()
        {
            var coinGeckoApiKey = ConfigurationManager.AppSettings["CoinGeckoApiKey"];

            using (HttpClient client = new HttpClient())
            {
                var request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri("https://api.coingecko.com/api/v3/simple/price?ids=the-open-network&vs_currencies=rub"),
                    Headers =
                    {
                        { "accept", "application/json" },
                        { "x-cg-demo-api-key", coinGeckoApiKey },
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
                    Debug.WriteLine($"Ошибка загрузки курса TON к RUB: {ex.Message}");
                }
            }
        }

        private string GeneratePdfReceipt(string customerFolderPath)
        {
            var organization = _organizationService.GetOrganization();

            string receiptPath = Path.Combine(customerFolderPath, $"Чек_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");

            using (PdfDocument document = new PdfDocument())
            {
                PdfPage page = document.AddPage();
                XGraphics gfx = XGraphics.FromPdfPage(page);
                XFont font = new XFont("Verdana", 10, XFontStyle.Regular);
                XFont boldFont = new XFont("Verdana", 10, XFontStyle.Bold);

                double yPoint = 0;

                // Заголовок
                gfx.DrawString("Товарный чек", boldFont, XBrushes.Black,
                    new XRect(0, yPoint, page.Width, 20),
                    XStringFormats.TopCenter);
                yPoint += 20;

                // Информация о компании
                gfx.DrawString(organization.Name, font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                gfx.DrawString($"ИНН: {organization.INN}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                gfx.DrawString(organization.Website, font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Дата
                gfx.DrawString($"Дата: {DateTime.Now:dd.MM.yyyy / HH:mm}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Разделитель
                gfx.DrawString("--------------------------------------------------------------", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Автомобиль
                gfx.DrawString($"Автомобиль: {Car.ModelName} {Car.TrimName} ({Car.Year})", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // VIN
                gfx.DrawString($"VIN: {MaskString(Car.VIN)}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Цвет
                gfx.DrawString($"Цвет: {Car.Color}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Мощность двигателя
                gfx.DrawString($"Мощность двигателя: {Car.HorsePower} л.с.", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Разделитель
                gfx.DrawString("--------------------------------------------------------------", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Расчет стоимости с НДС
                decimal priceWithoutVat = Car.Price / 1.2m; // Цена без НДС
                decimal vatAmount = Car.Price - priceWithoutVat; // Сумма НДС

                gfx.DrawString($"Цена без НДС: {priceWithoutVat:C}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                gfx.DrawString($"Сумма НДС (20%): {vatAmount:C}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                gfx.DrawString($"Итоговая цена: {Car.Price:C}", boldFont, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Разделитель
                gfx.DrawString("--------------------------------------------------------------", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Информация о клиенте
                gfx.DrawString($"ID Клиента: {SelectedCustomer.Id}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Разделитель
                gfx.DrawString("--------------------------------------------------------------", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Продавец и компания
                gfx.DrawString($"Продавец: {_userSessionService.CurrentUser.FullName}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                gfx.DrawString($"Компания: {organization.Name}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);
                yPoint += 20;

                // Уникальный номер
                gfx.DrawString($"Уникальный номер: {Guid.NewGuid()}", font, XBrushes.Black,
                    new XRect(20, yPoint, page.Width - 40, page.Height),
                    XStringFormats.TopLeft);

                document.Save(receiptPath);
            }

            return receiptPath;
        }

        private string MaskString(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length <= 4)
                return input;

            int length = input.Length;
            Random random = new Random();
            char[] masked = input.ToCharArray();

            for (int i = 2; i < length - 2; i++)
            {
                if (random.Next(0, 2) == 0)
                    masked[i] = '*';
            }

            return new string(masked);
        }








        private void GenerateRemainingDocuments(string customerFolderPath)
        {
            string tempFolder = Path.GetTempPath();
            string tempActPath = Path.Combine(tempFolder, "Акт приема-передачи_temp.docx");
            string tempSaleContractPath = Path.Combine(tempFolder, "Договор купли-продажи_temp.docx");

            SaveTemplateToFile("Акт приема-передачи", tempActPath);
            SaveTemplateToFile("Договор купли-продажи", tempSaleContractPath);

            WaitForFileAvailability(tempActPath);
            WaitForFileAvailability(tempSaleContractPath);

            var placeholders = GetPlaceholders();

            GenerateAndValidateDocument("Акт приема-передачи", tempActPath, placeholders);
            GenerateAndValidateDocument("Договор купли-продажи", tempSaleContractPath, placeholders);

            MoveAndRenameDocument(tempActPath, customerFolderPath, "Акт приема-передачи");
            MoveAndRenameDocument(tempSaleContractPath, customerFolderPath, "Договор купли-продажи");

            DeleteTemporaryFile(tempActPath);
            DeleteTemporaryFile(tempSaleContractPath);
        }

        private Dictionary<string, string> GetPlaceholders()
        {
            var organization = _organizationService.GetOrganization();
            var currentUser = _userSessionService.CurrentUser;
            string priceInWords = NumberToWordsConverter.Convert(Car.Price);
            string employeeFullName = currentUser.FullName;
            string employeeNameInInitials = ConvertToInitials(employeeFullName);
            string customerFullName = SelectedCustomer.FullName;
            string customerNameInInitials = ConvertToInitials(customerFullName);

            return new Dictionary<string, string>
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
                { "{{Телефон покупателя}}", SelectedCustomer?.Phone ?? string.Empty },

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

        public string CustomerFullName => SelectedCustomer != null ? $"{SelectedCustomer.FirstName} {SelectedCustomer.LastName}" : string.Empty;
        public string CustomerEmail => SelectedCustomer?.Email ?? string.Empty;
        public string CustomerPhone => SelectedCustomer?.Phone ?? string.Empty;

        public DateTime ContractDate { get; set; } = DateTime.Now;
        public string PlaceOfIssue { get; set; }
        public string City { get; set; }
        private static int contractNumberCounter = 1;
        public string ContractNumber => $"A{contractNumberCounter++.ToString("D5")}";

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

        private async Task SaveContract()
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
                // Создаем временный файл для предварительного договора
                string tempFolder = Path.GetTempPath();
                string tempContractPath = Path.Combine(tempFolder, "Предварительный договор купли-продажи_temp.docx");

                // Загружаем и сохраняем шаблон во временный файл
                SaveTemplateToFile("Предварительный договор купли-продажи", tempContractPath);

                // Проверка и ожидание готовности файла
                WaitForFileAvailability(tempContractPath);

                // Создаем папку для клиента
                string customerFolderName = $"{SelectedCustomer.Id}_{SelectedCustomer.LastName}_{SelectedCustomer.FirstName}_{SelectedCustomer.MiddleName}";
                string customerFolderPath = Path.Combine(destinationFolder, customerFolderName);
                Directory.CreateDirectory(customerFolderPath);

                // Генерация предварительного договора
                GenerateAndValidateDocument("Предварительный договор купли-продажи", tempContractPath, placeholders);

                // Перемещаем документ в папку клиента и переименовываем его
                string renamedContractPath = await MoveAndRenameDocument(tempContractPath, customerFolderPath, "Предварительный договор купли-продажи");

                // Удаление временного файла
                DeleteTemporaryFile(tempContractPath);

                // Обновляем флаг о генерации предварительного договора
                _isPreliminaryContractGenerated = true;
                ((CommunityToolkit.Mvvm.Input.RelayCommand)OpenPaymentDialogCommand).NotifyCanExecuteChanged();

                // Открываем проводник с папкой клиента
                OpenFolder(customerFolderPath);

                if (renamedContractPath != null)
                {
                    await ShowCustomMessageBox($"Предварительный договор купли-продажи для клиента {customerFullName} сохранен.", "Успех", CustomMessageBox.MessageKind.Success);
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
                Debug.WriteLine($"File {filename} is not ready: {ex.Message}");
                return false;
            }
        }

        private void WaitForFileAvailability(string filePath)
        {
            int attempts = 0;
            while (!IsFileReady(filePath) && attempts < 10)
            {
                System.Threading.Thread.Sleep(500);
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
                    return null;
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
            if (DialogHost.IsDialogOpen("RootDialogHost"))
            {
                Debug.WriteLine("Диалоговое окно уже открыто.");
                return false;
            }

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
    }
}
