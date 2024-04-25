using Npgsql;
using System;

namespace NextGen.src.Services
{
    public class DatabaseService
    {
        protected readonly string connectionString;

        public DatabaseService()
        {
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            try
            {
                var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                return connection;
            }
            catch (NpgsqlException ex)
            {
                throw new Exception("Ошибка подключения к базе данных: " + ex.Message);
            }
        }
    }
}
