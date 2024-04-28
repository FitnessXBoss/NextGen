using System.Windows;
using System.Windows.Controls;

namespace NextGen.src.UI.Helpers
{
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty =
            DependencyProperty.RegisterAttached(
                "BoundPassword",
                typeof(string),
                typeof(PasswordBoxHelper),
                new PropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject d)
        {
            return (string)d.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject d, string? value) // Разрешить null значения
        {
            if (value == null) value = string.Empty; // Предполагаем, что пароль не может быть null
            d.SetValue(BoundPasswordProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox? passwordBox = d as PasswordBox;
            if (passwordBox == null) return;

            string newPassword = (string)(e.NewValue ?? string.Empty); // Предотвращение передачи null
            passwordBox.PasswordChanged -= PasswordBox_PasswordChanged;
            if (!GetIsUpdating(passwordBox))
            {
                passwordBox.Password = newPassword;
            }
            passwordBox.PasswordChanged += PasswordBox_PasswordChanged;
        }



        private static void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox? passwordBox = sender as PasswordBox;
            if (passwordBox == null) return; // Проверка на null и выход

            SetIsUpdating(passwordBox, true);
            SetBoundPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }


        private static readonly DependencyProperty IsUpdatingProperty =
           DependencyProperty.RegisterAttached(
               "IsUpdating",
               typeof(bool),
               typeof(PasswordBoxHelper),
               new PropertyMetadata(false));

        private static bool GetIsUpdating(DependencyObject d)
        {
            return (bool)d.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject d, bool value)
        {
            d.SetValue(IsUpdatingProperty, value);
        }
    }
}
