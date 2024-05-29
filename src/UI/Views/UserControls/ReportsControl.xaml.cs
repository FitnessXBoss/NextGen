using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;


namespace NextGen.src.UI.Views.UserControls
{
    /// <summary>
    /// Логика взаимодействия для ReportsControl.xaml
    /// </summary>
    public partial class ReportsControl : UserControl
    {
        public ObservableCollection<double> SalesData { get; set; }
        public ObservableCollection<string> Months { get; set; }

        public ReportsControl()
        {
            InitializeComponent();
            SalesData = new ObservableCollection<double> { 20, 40, 50, 30, 60, 70 };
            Months = new ObservableCollection<string> { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь" };
            DataContext = this;
        }

        private void GenerateSalesReport(object sender, RoutedEventArgs e)
        {
            // Реализация генерации отчета о продажах
        }

        private void GenerateModelsReport(object sender, RoutedEventArgs e)
        {
            // Реализация генерации отчета по моделям
        }

        private void GenerateInventoryReport(object sender, RoutedEventArgs e)
        {
            // Реализация генерации отчета о наличии автомобилей
        }

        private void GenerateStockMovementReport(object sender, RoutedEventArgs e)
        {
            // Реализация генерации отчета о движении запасов
        }

        private void GenerateProfitLossReport(object sender, RoutedEventArgs e)
        {
            // Реализация генерации отчета о прибыли и убытках
        }

        private void GenerateDebtReport(object sender, RoutedEventArgs e)
        {
            // Реализация генерации отчета о дебиторской и кредиторской задолженности
        }

        private void GenerateCustomerLoyaltyReport(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateEmployeePerformanceReport(object sender, RoutedEventArgs e)
        {

        }

        private void GenerateWorkHoursReport(object sender, RoutedEventArgs e)
        {

        }

        

        private void GenerateFeedbackReport(object sender, RoutedEventArgs e)
        {

        }

        // Добавьте дополнительные обработчики для других кнопок


    }
}
