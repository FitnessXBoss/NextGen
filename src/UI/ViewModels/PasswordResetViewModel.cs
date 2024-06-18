using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using NextGen.src.Services.Security;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using NextGen.src.Services;
using NextGen.src.UI.Views.UserControls;

namespace NextGen.src.UI.ViewModels
{
    public class PasswordResetViewModel : INotifyPropertyChanged
    {
        private string? _username;
        private string? _twoFactorCode;
        private string? _newPassword;
        private bool _isCodeValid;
        private Visibility _inputFieldsVisibility = Visibility.Visible;

        public string Username
        {
            get { return _username!; }
            set { _username = value; }
        }

        public string TwoFactorCode
        {
            get { return _twoFactorCode!; }
            set { _twoFactorCode = value; }
        }

        public string NewPassword
        {
            get { return _newPassword!; }
            set { _newPassword = value; }
        }

        public bool IsCodeValid
        {
            get { return _isCodeValid; }
            set
            {
                _isCodeValid = value;
                OnPropertyChanged(nameof(IsCodeValid));
                InputFieldsVisibility = _isCodeValid ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public Visibility InputFieldsVisibility
        {
            get { return _inputFieldsVisibility; }
            set
            {
                _inputFieldsVisibility = value;
                OnPropertyChanged(nameof(InputFieldsVisibility));
            }
        }

        public ICommand ResetPasswordCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand VerifyCodeCommand { get; private set; }

        public PasswordResetViewModel()
        {
            ResetPasswordCommand = new RelayCommand(async () => await ResetPasswordAsync(), () => IsCodeValid);
            CancelCommand = new RelayCommand(Cancel);
            VerifyCodeCommand = new RelayCommand(async () => await VerifyCodeAsync());
        }

        private async Task VerifyCodeAsync()
        {
            AuthService authService = new AuthService();
            var userId = await GetUserIdByUsername(Username);

            if (userId == null)
            {
                await ShowCustomMessageBox("Пользователь не найден.", "Ошибка", CustomMessageBox.MessageKind.Error);
                return;
            }

            var (isValid, errorMessage) = await authService.ValidateTwoFactorCodeAsync(userId.Value, TwoFactorCode);

            if (isValid)
            {
                IsCodeValid = true;
                await ShowCustomMessageBox("Код подтвержден. Теперь вы можете сменить пароль.", "Успех", CustomMessageBox.MessageKind.Success);
            }
            else
            {
                IsCodeValid = false;
                await ShowCustomMessageBox(errorMessage ?? "Неверный код двухфакторной аутентификации.", "Ошибка", CustomMessageBox.MessageKind.Error);
            }

            // Обновление состояния команды ResetPasswordCommand, чтобы включить или отключить кнопку
            ((RelayCommand)ResetPasswordCommand).NotifyCanExecuteChanged();
        }

        private async Task ResetPasswordAsync()
        {
            AuthService authService = new AuthService();
            var userId = await GetUserIdByUsername(Username);

            if (userId == null)
            {
                await ShowCustomMessageBox("Пользователь не найден.", "Ошибка", CustomMessageBox.MessageKind.Error);
                return;
            }

            var employeeId = await GetEmployeeIdByUsername(Username);
            if (employeeId == null)
            {
                await ShowCustomMessageBox("Сотрудник не найден.", "Ошибка", CustomMessageBox.MessageKind.Error);
                return;
            }

            var result = await authService.UpdateUserCredentialsAsync(employeeId.Value, NewPassword);
            if (result)
            {
                await ShowCustomMessageBox("Пароль успешно сброшен.", "Успех", CustomMessageBox.MessageKind.Success);
                CloseDialog();
            }
            else
            {
                await ShowCustomMessageBox("Ошибка при сбросе пароля.", "Ошибка", CustomMessageBox.MessageKind.Error);
            }
        }

        private void Cancel()
        {
            CloseDialog();
        }

        private async Task<int?> GetUserIdByUsername(string username)
        {
            try
            {
                using (var conn = await new DatabaseService().GetConnectionAsync())
                {
                    using (var cmd = new NpgsqlCommand("SELECT user_id FROM users WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                return reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user ID: {ex.Message}");
            }
            return null;
        }

        private async Task<int?> GetEmployeeIdByUsername(string username)
        {
            try
            {
                using (var conn = await new DatabaseService().GetConnectionAsync())
                {
                    using (var cmd = new NpgsqlCommand("SELECT employee_id FROM users WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (reader.Read())
                            {
                                return reader.GetInt32(0);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting employee ID: {ex.Message}");
            }
            return null;
        }

        private async Task ShowCustomMessageBox(string message, string title, CustomMessageBox.MessageKind kind)
        {
            var view = new CustomMessageBox(message, title, kind);
            await DialogHost.Show(view, "PasswordResetDialog");
        }

        private void CloseDialog()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                DialogHost.CloseDialogCommand.Execute(null, null);
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
