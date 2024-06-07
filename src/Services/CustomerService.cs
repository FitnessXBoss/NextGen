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
                var cmd = new NpgsqlCommand("SELECT customer_id, first_name, last_name, date_of_birth, passport_number, email, phone, address, created_by FROM customers", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("customer_id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("date_of_birth")),
                            PassportNumber = reader.GetString(reader.GetOrdinal("passport_number")),
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
                var cmd = new NpgsqlCommand("INSERT INTO customers (first_name, last_name, date_of_birth, passport_number, email, phone, address, created_by) VALUES (@first_name, @last_name, @date_of_birth, @passport_number, @email, @phone, @address, @created_by) RETURNING customer_id", connection);
                cmd.Parameters.AddWithValue("first_name", newCustomer.FirstName);
                cmd.Parameters.AddWithValue("last_name", newCustomer.LastName);
                cmd.Parameters.AddWithValue("date_of_birth", newCustomer.DateOfBirth);
                cmd.Parameters.AddWithValue("passport_number", newCustomer.PassportNumber);
                cmd.Parameters.AddWithValue("email", newCustomer.Email);
                cmd.Parameters.AddWithValue("phone", newCustomer.Phone);
                cmd.Parameters.AddWithValue("address", newCustomer.Address);
                cmd.Parameters.AddWithValue("created_by", createdBy);

                newCustomer.Id = (int)cmd.ExecuteScalar();
            }
            return newCustomer;
        }
    }
}
