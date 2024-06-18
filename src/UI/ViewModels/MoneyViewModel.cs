using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using NextGen.src.Services;
using NextGen.src.UI;
using NextGen.src.UI.Models.NextGen.src.Data.Database.Models;

namespace NextGen.src.UI.ViewModels
{
    internal class MoneyViewModel : BaseViewModel
    {
        private readonly MoneyService _moneyService;
        private readonly OrganizationService _organizationService;
        private decimal _totalBalance;
        private decimal _monthlyIncome;
        private decimal _yearlyIncome;
        private decimal _yearlyExpense;
        private decimal _tonToRubRate = 1;  // Установим значение по умолчанию, чтобы избежать деления на ноль
        private Organization _organization;
        private DateTime? _startDate;
        private DateTime? _endDate;
        private DispatcherTimer _timer;

        public ObservableCollection<Payment> Payments { get; set; }
        public ObservableCollection<Payment> FilteredPayments { get; set; }
        public ObservableCollection<Expense> Expenses { get; set; }
        public ObservableCollection<Expense> FilteredExpenses { get; set; }

        public ICommand RefreshDataCommand { get; }

        public decimal TotalBalance
        {
            get => _totalBalance;
            set
            {
                _totalBalance = value;
                OnPropertyChanged(nameof(TotalBalance));
                OnPropertyChanged(nameof(TotalBalanceInTon));
            }
        }

        public decimal MonthlyIncome
        {
            get => _monthlyIncome;
            set
            {
                _monthlyIncome = value;
                OnPropertyChanged(nameof(MonthlyIncome));
                OnPropertyChanged(nameof(MonthlyIncomeInTon));
            }
        }

        public decimal YearlyIncome
        {
            get => _yearlyIncome;
            set
            {
                _yearlyIncome = value;
                OnPropertyChanged(nameof(YearlyIncome));
                OnPropertyChanged(nameof(YearlyIncomeInTon));
            }
        }

        public decimal YearlyExpense
        {
            get => _yearlyExpense;
            set
            {
                _yearlyExpense = value;
                OnPropertyChanged(nameof(YearlyExpense));
                OnPropertyChanged(nameof(YearlyExpenseInTon));
            }
        }

        public decimal TonToRubRate
        {
            get => _tonToRubRate;
            set
            {
                if (value == 0)
                {
                    value = 1;  // Предотвращение деления на ноль
                }
                _tonToRubRate = value;
                OnPropertyChanged(nameof(TonToRubRate));
                OnPropertyChanged(nameof(TotalBalanceInTon));
                OnPropertyChanged(nameof(MonthlyIncomeInTon));
                OnPropertyChanged(nameof(YearlyIncomeInTon));
                OnPropertyChanged(nameof(YearlyExpenseInTon));
            }
        }

        public decimal TotalBalanceInTon => TotalBalance / TonToRubRate;
        public decimal MonthlyIncomeInTon => MonthlyIncome / TonToRubRate;
        public decimal YearlyIncomeInTon => YearlyIncome / TonToRubRate;
        public decimal YearlyExpenseInTon => YearlyExpense / TonToRubRate;

        public Organization Organization
        {
            get => _organization;
            set
            {
                _organization = value;
                OnPropertyChanged(nameof(Organization));
            }
        }

        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
                FilterData();
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
                FilterData();
            }
        }

        public MoneyViewModel()
        {
            _moneyService = new MoneyService();
            _organizationService = new OrganizationService();

            Payments = new ObservableCollection<Payment>(_moneyService.GetPayments());
            FilteredPayments = new ObservableCollection<Payment>(Payments);
            Expenses = new ObservableCollection<Expense>(_moneyService.GetExpenses());
            FilteredExpenses = new ObservableCollection<Expense>(Expenses);
            TotalBalance = _moneyService.GetTotalBalance();
            Organization = _organizationService.GetOrganization();
            LoadTonToRubRate();
            CalculateIncomesAndExpenses();

            // Setup the timer for periodic updates
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _timer.Tick += (sender, args) => RefreshData();
            _timer.Start();

            RefreshDataCommand = new RelayCommand(RefreshData);
        }

        private async void LoadTonToRubRate()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("https://api.coingecko.com/api/v3/simple/price?ids=the-open-network&vs_currencies=rub");
                    response.EnsureSuccessStatusCode();
                    var responseBody = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(responseBody))
                    {
                        if (doc.RootElement.TryGetProperty("the-open-network", out JsonElement tonElement) &&
                            tonElement.TryGetProperty("rub", out JsonElement rubElement) &&
                            rubElement.TryGetDecimal(out decimal rubValue))
                        {
                            TonToRubRate = rubValue;
                        }
                        else
                        {
                            throw new Exception("Invalid rate info received.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle the error (e.g., log it, show a message to the user, etc.)
                }
            }
        }

        private void CalculateIncomesAndExpenses()
        {
            DateTime now = DateTime.Now;

            MonthlyIncome = Payments.Where(p => p.PaymentDate.Month == now.Month && p.PaymentDate.Year == now.Year)
                                    .Sum(p => p.Amount);
            YearlyIncome = Payments.Where(p => p.PaymentDate.Year == now.Year)
                                   .Sum(p => p.Amount);
            YearlyExpense = Expenses.Where(e => e.Date.Year == now.Year)
                                   .Sum(e => e.Amount);
        }

        private void FilterData()
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                var start = StartDate.Value.Date;
                var end = EndDate.Value.Date;

                FilteredPayments = new ObservableCollection<Payment>(Payments.Where(p => p.PaymentDate.Date >= start && p.PaymentDate.Date <= end));
                FilteredExpenses = new ObservableCollection<Expense>(Expenses.Where(e => e.Date.Date >= start && e.Date.Date <= end));
            }
            else
            {
                FilteredPayments = new ObservableCollection<Payment>(Payments);
                FilteredExpenses = new ObservableCollection<Expense>(Expenses);
            }

            OnPropertyChanged(nameof(FilteredPayments));
            OnPropertyChanged(nameof(FilteredExpenses));
        }

        public void RefreshData()
        {
            Payments = new ObservableCollection<Payment>(_moneyService.GetPayments());
            Expenses = new ObservableCollection<Expense>(_moneyService.GetExpenses());
            FilterData();
            CalculateIncomesAndExpenses();
            TotalBalance = _moneyService.GetTotalBalance();
            OnPropertyChanged(nameof(Payments));
            OnPropertyChanged(nameof(Expenses));
            OnPropertyChanged(nameof(TotalBalance));
        }
    }
}
