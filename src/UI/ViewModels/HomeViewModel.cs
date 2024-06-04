using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows;
using NextGen.src.Services;
using Npgsql;

namespace NextGen.src.UI.ViewModels
{
    public class HomeViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<User> Users { get; private set; } = new ObservableCollection<User>();
        private DatabaseService _databaseService;

        public HomeViewModel() : this(new DatabaseService()) // Или передайте сюда нужный экземпляр DatabaseService
        {
        }

        public HomeViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            LoadData();
        }

        private int _onlineCount;
        public int OnlineCount
        {
            get => _onlineCount;
            set
            {
                _onlineCount = value;
                OnPropertyChanged();
            }
        }

        private int _totalSales;
        public int TotalSales
        {
            get => _totalSales;
            set
            {
                _totalSales = value;
                OnPropertyChanged();
            }
        }

        public void RefreshData()
        {
            LoadData();
        }

        private async void LoadData()
        {
            var tempList = new List<User>();

            // Загрузка информации о пользователях
            using (var conn = _databaseService.GetConnection())
            {
               
                var cmd = new NpgsqlCommand("SELECT e.first_name, e.last_name, r.role_name, e.photo_url, u.is_online FROM employees e JOIN users u ON e.employee_id = u.employee_id JOIN roles r ON e.role_id = r.role_id", conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var user = new User
                        {
                            FirstName = reader["first_name"] as string,
                            LastName = reader["last_name"] as string,
                            UserRole = reader["role_name"] as string,
                            UserPhotoUrl = reader.IsDBNull(reader.GetOrdinal("photo_url")) ? null : reader["photo_url"] as string,
                            IsOnline = Convert.ToBoolean(reader["is_online"])
                        };
                        tempList.Add(user);
                    }
                }
            }

            int salesCount = 0;
            // Загрузка данных о продажах
            using (var conn = _databaseService.GetConnection())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }
                var salesCmd = new NpgsqlCommand("SELECT COUNT(*) FROM sales", conn);
                salesCount = Convert.ToInt32(await salesCmd.ExecuteScalarAsync());

            }

            TotalSales = salesCount;  // Обновление общего числа продаж
            var sortedUsers = tempList.OrderByDescending(user => user.IsOnline).ThenBy(user => user.FirstName).ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                Users.Clear();
                foreach (var user in sortedUsers)
                {
                    Users.Add(user);
                }
                OnlineCount = Users.Count(u => u.IsOnline);  // Обновление количества онлайн пользователей
            });
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class User
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserRole { get; set; }
        public string? UserPhotoUrl { get; set; }
        public bool IsOnline { get; set; }
        public string UserName => $"{FirstName} {LastName}";
        public SolidColorBrush StatusColor => IsOnline ? new SolidColorBrush(Colors.Green) : new SolidColorBrush(Colors.Red);
    }
}
