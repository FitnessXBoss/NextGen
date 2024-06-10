using Npgsql;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace NextGen.src.Services
{
    public class DatabaseService
    {
        private readonly string connectionString;
        private const int MaxRetries = 3;
        private const int RetryDelay = 2000; // Задержка между попытками в миллисекундах

        public DatabaseService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    var connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync();
                    return connection;
                }
                catch (NpgsqlException ex) when (ex.InnerException is IOException || ex.InnerException is SocketException)
                {
                    if (attempt == MaxRetries)
                    {
                        throw;
                    }
                    await Task.Delay(RetryDelay);
                }
            }
            throw new Exception("Не удалось подключиться к базе данных после нескольких попыток.");
        }

        private async Task EnsureConnectionOpenAsync(NpgsqlConnection connection)
        {
            if (connection.State == System.Data.ConnectionState.Closed || connection.State == System.Data.ConnectionState.Broken)
            {
                await connection.OpenAsync();
            }
        }

        public async Task<NpgsqlDataReader> ExecuteReaderWithRetryAsync(NpgsqlCommand cmd)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    await EnsureConnectionOpenAsync(cmd.Connection);
                    return await cmd.ExecuteReaderAsync();
                }
                catch (NpgsqlException ex) when (ex.InnerException is IOException || ex.InnerException is SocketException)
                {
                    if (attempt == MaxRetries)
                    {
                        throw;
                    }
                    await Task.Delay(RetryDelay);
                    cmd.Connection = await GetConnectionAsync(); // Переподключение
                }
            }
            throw new Exception("Не удалось выполнить запрос после нескольких попыток.");
        }

        public async Task<object> ExecuteScalarWithRetryAsync(NpgsqlCommand cmd)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    await EnsureConnectionOpenAsync(cmd.Connection);
                    return await cmd.ExecuteScalarAsync();
                }
                catch (NpgsqlException ex) when (ex.InnerException is IOException || ex.InnerException is SocketException)
                {
                    if (attempt == MaxRetries)
                    {
                        throw;
                    }
                    await Task.Delay(RetryDelay);
                    cmd.Connection = await GetConnectionAsync(); // Переподключение
                }
            }
            throw new Exception("Не удалось выполнить запрос после нескольких попыток.");
        }
    }
}
