using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using NextGen.src.Data.Database.Models;

namespace NextGen.src.Services
{
    public class MoneyService
    {
        private readonly string connectionString;

        public MoneyService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public IEnumerable<Payment> GetPayments()
        {
            var payments = new List<Payment>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT * FROM payments", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        payments.Add(new Payment
                        {
                            PaymentId = reader.GetInt32(reader.GetOrdinal("payment_id")),
                            Sender = reader.GetString(reader.GetOrdinal("sender")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                            PaymentDate = reader.GetDateTime(reader.GetOrdinal("payment_date")),
                            CarId = reader.IsDBNull(reader.GetOrdinal("car_id")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("car_id"))
                        });
                    }
                }
            }
            return payments;
        }

        public IEnumerable<Expense> GetExpenses()
        {
            var expenses = new List<Expense>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT * FROM expenses", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        expenses.Add(new Expense
                        {
                            ExpenseId = reader.GetInt32(reader.GetOrdinal("expense_id")),
                            Description = reader.GetString(reader.GetOrdinal("description")),
                            Amount = reader.GetDecimal(reader.GetOrdinal("amount")),
                            Date = reader.GetDateTime(reader.GetOrdinal("date"))
                        });
                    }
                }
            }
            return expenses;
        }

        public decimal GetTotalBalance()
        {
            decimal totalIncome = 0, totalExpense = 0;

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var incomeCmd = new NpgsqlCommand("SELECT SUM(amount) FROM payments", connection);
                var incomeResult = incomeCmd.ExecuteScalar();
                if (incomeResult != DBNull.Value)
                    totalIncome = Convert.ToDecimal(incomeResult);

                var expenseCmd = new NpgsqlCommand("SELECT SUM(amount) FROM expenses", connection);
                var expenseResult = expenseCmd.ExecuteScalar();
                if (expenseResult != DBNull.Value)
                    totalExpense = Convert.ToDecimal(expenseResult);
            }

            return totalIncome - totalExpense;
        }
    }


    public class Payment
    {
        public int PaymentId { get; set; }
        public string Sender { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public int? CarId { get; set; }
    }

    public class Expense
    {
        public int ExpenseId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}
