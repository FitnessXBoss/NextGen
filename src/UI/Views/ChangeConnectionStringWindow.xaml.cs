using NextGen.src.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NextGen.src.UI.Views
{
    public partial class ChangeConnectionStringWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> SavedConnectionStrings { get; set; }

        private string _selectedConnectionString;
        private string _connectionName;
        private string _host;
        private string _port;
        private string _database;
        private string _username;
        private string _password;
        private string _fullConnectionString;

        private bool _updatingFullConnectionString;
        private bool _updatingParts;

        public string SelectedConnectionString
        {
            get { return _selectedConnectionString; }
            set
            {
                if (_updatingParts) return;
                _updatingFullConnectionString = true;
                _selectedConnectionString = value;
                OnPropertyChanged(nameof(SelectedConnectionString));
                LoadSelectedConnectionString();
                _updatingFullConnectionString = false;
            }
        }

        public string ConnectionName
        {
            get { return _connectionName; }
            set
            {
                _connectionName = value;
                OnPropertyChanged(nameof(ConnectionName));
            }
        }

        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                OnPropertyChanged(nameof(Host));
                UpdateFullConnectionString();
            }
        }

        public string Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
                UpdateFullConnectionString();
            }
        }

        public string Database
        {
            get { return _database; }
            set
            {
                _database = value;
                OnPropertyChanged(nameof(Database));
                UpdateFullConnectionString();
            }
        }

        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
                UpdateFullConnectionString();
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                UpdateFullConnectionString();
            }
        }

        public string FullConnectionString
        {
            get { return _fullConnectionString; }
            set
            {
                if (_updatingFullConnectionString) return;
                _updatingParts = true;
                _fullConnectionString = value;
                OnPropertyChanged(nameof(FullConnectionString));
                ParseFullConnectionString();
                _updatingParts = false;
            }
        }

        public ChangeConnectionStringWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Инициализация значений по умолчанию
            SavedConnectionStrings = new ObservableCollection<string>(ConnectionService.GetSavedConnectionStrings());
            SelectedConnectionString = SavedConnectionStrings.FirstOrDefault() ?? string.Empty;
            ConnectionName = string.Empty;
            Host = string.Empty;
            Port = string.Empty;
            Database = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            FullConnectionString = string.Empty;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ConnectionName))
            {
                MessageBox.Show("Имя подключения не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (SavedConnectionStrings.Contains(ConnectionName))
            {
                ConnectionService.RemoveConnectionString(ConnectionName);
            }

            var connectionString = $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";

            if (!ConnectionService.ValidateConnectionString(connectionString))
            {
                MessageBox.Show("Неверный формат строки подключения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var encryptedConnectionString = Encrypt(connectionString, ConnectionService.EncryptionKey);

            ConnectionService.SaveConnectionString(ConnectionName, encryptedConnectionString);
            if (!SavedConnectionStrings.Contains(ConnectionName))
            {
                SavedConnectionStrings.Add(ConnectionName);
            }
            ConnectionService.SetCurrentConnectionString(ConnectionName); // Устанавливаем текущую строку подключения
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = ((PasswordBox)sender).Password;
            UpdateFullConnectionString();
        }

        public static string Encrypt(string plainText, string key)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string Decrypt(string cipherText, string key)
        {
            try
            {
                byte[] iv = new byte[16];
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(key);
                    aes.IV = iv;
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream memoryStream = new MemoryStream(buffer))
                    {
                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            {
                                return streamReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Ошибка расшифровки строки подключения. Строка не является допустимой строкой Base-64.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return string.Empty;
            }
        }

        private void UpdateFullConnectionString()
        {
            if (_updatingParts) return;
            _updatingFullConnectionString = true;
            FullConnectionString = $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password};";
            _updatingFullConnectionString = false;
        }

        private void ParseFullConnectionString()
        {
            if (string.IsNullOrEmpty(FullConnectionString)) return;

            var parts = FullConnectionString.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0].ToLower())
                    {
                        case "host":
                            Host = keyValue[1];
                            break;
                        case "port":
                            Port = keyValue[1];
                            break;
                        case "database":
                            Database = keyValue[1];
                            break;
                        case "username":
                            Username = keyValue[1];
                            break;
                        case "password":
                            Password = keyValue[1];
                            break;
                    }
                }
            }
        }

        private void LoadSelectedConnectionString()
        {
            if (string.IsNullOrEmpty(SelectedConnectionString)) return;

            var connectionString = ConnectionService.GetConnectionStringByName(SelectedConnectionString);
            if (string.IsNullOrEmpty(connectionString)) return;

            // Устанавливаем имя подключения при выборе
            ConnectionName = SelectedConnectionString;

            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                var keyValue = part.Split('=');
                if (keyValue.Length == 2)
                {
                    switch (keyValue[0].ToLower())
                    {
                        case "host":
                            Host = keyValue[1];
                            break;
                        case "port":
                            Port = keyValue[1];
                            break;
                        case "database":
                            Database = keyValue[1];
                            break;
                        case "username":
                            Username = keyValue[1];
                            break;
                        case "password":
                            Password = keyValue[1];
                            break;
                    }
                }
            }
            UpdateFullConnectionString();
        }

        private void ConnectionStringComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadSelectedConnectionString();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
