using System.Collections.Generic;
using System.Configuration;
using Npgsql;
using NextGen.src.Data.Database.Models;

namespace NextGen.src.Services
{
    public class CustomerService
    {
        private readonly string connectionString;

        public CustomerService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public IEnumerable<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT customer_id, first_name, middle_name, last_name, date_of_birth, passport_number, passport_issue_date, passport_issuer, email, phone, address, created_by FROM customers", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("customer_id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            MiddleName = reader.GetString(reader.GetOrdinal("middle_name")), // Добавлено чтение нового поля
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                            PassportNumber = reader.GetString(reader.GetOrdinal("passport_number")),
                            PassportIssueDate = reader.GetDateTime(reader.GetOrdinal("passport_issue_date")),
                            PassportIssuer = reader.GetString(reader.GetOrdinal("passport_issuer")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            Phone = reader.GetString(reader.GetOrdinal("phone")),
                            Address = reader.GetString(reader.GetOrdinal("address")),
                            CreatedBy = reader.GetInt32(reader.GetOrdinal("created_by"))
                        });
                    }
                }
            }
            return customers;
        }

        public Customer AddCustomer(Customer newCustomer, int createdBy)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("INSERT INTO customers (first_name, middle_name, last_name, date_of_birth, passport_number, passport_issue_date, passport_division_code, passport_issuer, place_of_birth, email, phone, address, created_by) VALUES (@first_name, @middle_name, @last_name, @date_of_birth, @passport_number, @passport_issue_date, @passport_division_code, @passport_issuer, @place_of_birth, @email, @phone, @address, @created_by) RETURNING customer_id", connection);
                cmd.Parameters.AddWithValue("first_name", newCustomer.FirstName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("middle_name", newCustomer.MiddleName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("last_name", newCustomer.LastName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("date_of_birth", newCustomer.DateOfBirth);
                cmd.Parameters.AddWithValue("passport_number", newCustomer.PassportNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("passport_issue_date", newCustomer.PassportIssueDate);
                cmd.Parameters.AddWithValue("passport_division_code", newCustomer.PassportDivisionCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("passport_issuer", newCustomer.PassportIssuer ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("place_of_birth", newCustomer.PlaceOfBirth ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("email", newCustomer.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("phone", newCustomer.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("address", newCustomer.Address ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("created_by", createdBy);

                newCustomer.Id = (int)cmd.ExecuteScalar();
            }
            return newCustomer;
        }

        public enum CustomerExistenceType
        {
            None,
            PassportNumber,
            Email,
            Phone
        }

        public CustomerExistenceType CheckCustomerExistence(string passportNumber, string email, string phone)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT passport_number, email, phone FROM customers WHERE passport_number = @passport_number OR email = @email OR phone = @phone", connection);
                cmd.Parameters.AddWithValue("passport_number", NpgsqlTypes.NpgsqlDbType.Varchar, passportNumber ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("email", NpgsqlTypes.NpgsqlDbType.Varchar, email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("phone", NpgsqlTypes.NpgsqlDbType.Varchar, phone ?? (object)DBNull.Value);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader["passport_number"].ToString() == passportNumber)
                        {
                            return CustomerExistenceType.PassportNumber;
                        }
                        if (reader["email"].ToString() == email)
                        {
                            return CustomerExistenceType.Email;
                        }
                        if (reader["phone"].ToString() == phone)
                        {
                            return CustomerExistenceType.Phone;
                        }
                    }
                }
            }
            return CustomerExistenceType.None;
        }
    }
}
