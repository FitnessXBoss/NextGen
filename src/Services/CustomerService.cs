using Npgsql;
using System.Collections.Generic;
using System.Configuration;
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

        public List<Customer> GetAllCustomers()
        {
            var customers = new List<Customer>();

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT customer_id, first_name, last_name, email, phone FROM customers", connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        customers.Add(new Customer
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("customer_id")),
                            FirstName = reader.GetString(reader.GetOrdinal("first_name")),
                            LastName = reader.GetString(reader.GetOrdinal("last_name")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            Phone = reader.GetString(reader.GetOrdinal("phone"))
                        });
                    }
                }
            }

            return customers;
        }
    }
}
