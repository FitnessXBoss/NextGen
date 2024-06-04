using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using NextGen.src.UI.ViewModels;

namespace NextGen.src.UI.Views
{
    public partial class DashboardWindow : Window
    {
        public DashboardWindow()
        {
            InitializeComponent();
            Loaded += DashboardWindow_Loaded;
            this.SourceInitialized += new EventHandler(Window_SourceInitialized);
        }

        private async void DashboardWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Убедитесь, что конструктор ViewModel теперь приватный и он загружает данные асинхронно через CreateAsync
            var viewModel = await DashboardViewModel.CreateAsync();
            DataContext = viewModel;
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Header_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ShouldHandleDoubleClick(e))
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    this.WindowState = WindowState.Normal;
                }
                else
                {
                    MaximizeWindow();
                }
            }
        }

        private bool ShouldHandleDoubleClick(MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as DependencyObject;

            while (element != null)
            {
                if (element is Button || element is Image || element is TextBlock || element is PopupBox)
                {
                    return false;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return true;
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            this.StateChanged += new EventHandler(Window_StateChanged);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                MaximizeWindow();
            }
        }

        private void MaximizeWindow()
        {
            // Используем размеры рабочего стола
            this.MaxWidth = SystemParameters.WorkArea.Width;
            this.MaxHeight = SystemParameters.WorkArea.Height;
            this.WindowState = WindowState.Maximized;
        }
    }
}
