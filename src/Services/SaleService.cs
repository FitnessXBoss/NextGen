using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using NextGen.src.Data.Database.Models;

namespace NextGen.src.Services
{
    public class SaleService
    {
        private readonly string connectionString;

        public SaleService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public void RecordSale(int carId, int userId, int customerId, decimal amount)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Вставка данных о продаже
                var cmd = new NpgsqlCommand("INSERT INTO sales (car_id, user_id, customer_id, sale_date, amount) VALUES (@car_id, @user_id, @customer_id, @sale_date, @amount)", connection);
                cmd.Parameters.AddWithValue("car_id", carId);
                cmd.Parameters.AddWithValue("user_id", userId);
                cmd.Parameters.AddWithValue("customer_id", customerId);
                cmd.Parameters.AddWithValue("sale_date", DateTime.Now);
                cmd.Parameters.AddWithValue("amount", amount);
                cmd.ExecuteNonQuery();

                // Обновление статуса автомобиля
                var updateCmd = new NpgsqlCommand("UPDATE cars SET status = 'Sold' WHERE car_id = @car_id", connection);
                updateCmd.Parameters.AddWithValue("car_id", carId);
                updateCmd.ExecuteNonQuery();
            }
        }

        public IEnumerable<Sale> GetSalesByCustomerId(int customerId)
        {
            var sales = new List<Sale>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT * FROM sales WHERE customer_id = @customer_id", connection);
                cmd.Parameters.AddWithValue("customer_id", customerId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        sales.Add(new Sale
                        {
                            SaleId = reader.GetInt32(reader.GetOrdinal("sale_id")),
                            CarId = reader.GetInt32(reader.GetOrdinal("car_id")),
                            SaleDate = reader.GetDateTime(reader.GetOrdinal("sale_date")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                            UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("customer_id"))
                        });
                    }
                }
            }
            return sales;
        }
    }
}
