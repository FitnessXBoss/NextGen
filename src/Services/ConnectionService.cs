using NextGen.src.UI.Views;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NextGen.src.Services
{
    public class ConnectionService
    {
        public static readonly string EncryptionKey;
        private static Dictionary<string, string> _connectionStrings = new Dictionary<string, string>();
        private static string _currentConnectionString;

        static ConnectionService()
        {
            EncryptionKey = GetOrCreateEncryptionKey();
            LoadConnectionStrings();
        }

        public static void SaveConnectionString(string name, string encryptedConnectionString)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Имя строки подключения не может быть пустым.", nameof(name));

            if (_connectionStrings.ContainsKey(name))
            {
                _connectionStrings[name] = encryptedConnectionString;
            }
            else
            {
                _connectionStrings.Add(name, encryptedConnectionString);
            }
            SaveConnectionStringsToConfig();
        }

        public static void RemoveConnectionString(string name)
        {
            if (_connectionStrings.ContainsKey(name))
            {
                _connectionStrings.Remove(name);
                SaveConnectionStringsToConfig();
            }
        }

        public static List<string> GetSavedConnectionStrings()
        {
            return _connectionStrings.Keys.ToList();
        }

        public static string GetConnectionStringByName(string name)
        {
            return _connectionStrings.TryGetValue(name, out var encryptedConnectionString)
                ? DecryptConnectionString(encryptedConnectionString)
                : null;
        }

        public static string GetCurrentConnectionString()
        {
            return _currentConnectionString;
        }

        public static void SetCurrentConnectionString(string name)
        {
            _currentConnectionString = GetConnectionStringByName(name);
        }

        private static void LoadConnectionStrings()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");

            if (connectionStringsSection != null)
            {
                foreach (ConnectionStringSettings connectionString in connectionStringsSection.ConnectionStrings)
                {
                    if (connectionString.Name != "LocalSqlServer")
                    {
                        _connectionStrings[connectionString.Name] = connectionString.ConnectionString;
                    }
                }
            }
        }

        private static void SaveConnectionStringsToConfig()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var connectionStringsSection = (ConnectionStringsSection)config.GetSection("connectionStrings");

            if (connectionStringsSection != null)
            {
                connectionStringsSection.ConnectionStrings.Clear();

                foreach (var connectionString in _connectionStrings)
                {
                    connectionStringsSection.ConnectionStrings.Add(new ConnectionStringSettings(connectionString.Key, connectionString.Value));
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");
            }
        }

        public static string DecryptConnectionString(string encryptedConnectionString)
        {
            var decrypted = ChangeConnectionStringWindow.Decrypt(encryptedConnectionString, EncryptionKey);

            // Проверка правильности строки подключения
            Debug.WriteLine("Decrypted Connection String: " + decrypted);

            return decrypted;
        }

        private static string GetOrCreateEncryptionKey()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var appSettingsSection = (AppSettingsSection)config.GetSection("appSettings");

            if (appSettingsSection.Settings["EncryptionKey"] == null)
            {
                var key = GenerateEncryptionKey();
                appSettingsSection.Settings.Add("EncryptionKey", key);
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
                return key;
            }

            return appSettingsSection.Settings["EncryptionKey"].Value;
        }

        private static string GenerateEncryptionKey()
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] key = new byte[32]; // 256 bit key for AES-256
                rng.GetBytes(key);
                return Convert.ToBase64String(key);
            }
        }

        public static bool ValidateConnectionString(string connectionString)
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }
    }
}
