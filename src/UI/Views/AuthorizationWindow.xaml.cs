using NextGen.src.Services.Security;
using NextGen.src.UI.ViewModels;
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
using System.Windows.Shapes;

namespace NextGen.src.UI.Views
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
            var viewModel = new AuthorizationViewModel();
            viewModel.CloseAction = new Action(this.Close);
            this.DataContext = viewModel;


        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ResetAuthText_MouseDown(object sender, MouseButtonEventArgs e)
        {

            MessageBox.Show("Функция восстановления пароля находится в разработке.");
            
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void Btn_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
