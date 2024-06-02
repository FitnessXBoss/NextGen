using Npgsql;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace NextGen.src.Services.Security
{
    public class AuthService : DatabaseService
    {
        public AuthService() : base() { }

        public (UserAuthData?, string?) AuthenticateUser(string username, string password)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    using (var cmd = new NpgsqlCommand("SELECT user_id, username, password_hash, salt, employee_id FROM users WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var storedHash = reader.GetString(2);
                                var storedSalt = reader.GetString(3);
                                var saltBytes = Convert.FromBase64String(storedSalt);
                                var hashOfEnteredPassword = HashPassword(password, saltBytes);

                                if (storedHash == hashOfEnteredPassword)
                                {
                                    var user = new UserAuthData
                                    {
                                        UserId = reader.GetInt32(0),
                                        Username = reader.GetString(1),
                                        EmployeeId = reader.GetInt32(4)
                                    };
                                    UserSessionService.Instance.SetCurrentUser(user); // Устанавливаем текущего пользователя
                                    return (user, null);
                                }
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                return (null, "Ошибка подключения к базе данных!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication error: {ex.Message}");
            }
            return (null, "Неверное имя пользователя или пароль!");
        }




        public bool RegisterUser(string username, string password)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new NpgsqlCommand("INSERT INTO users (username, password_hash, salt) VALUES (@username, @hash, @salt)", conn))
                    {
                        var salt = GenerateSalt();
                        var hash = HashPassword(password, salt);
                        var saltBase64 = Convert.ToBase64String(salt);

                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@hash", hash);
                        cmd.Parameters.AddWithValue("@salt", saltBase64);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибок базы данных или других исключений
                Console.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public bool UpdateUserCredentials(int employeeId, string newPassword)
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    byte[] newSalt = GenerateSalt();
                    string newPasswordHash = HashPassword(newPassword, newSalt);
                    string saltBase64 = Convert.ToBase64String(newSalt);

                    using (var cmd = new NpgsqlCommand("UPDATE users SET password_hash = @password, salt = @salt WHERE employee_id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@password", newPasswordHash);
                        cmd.Parameters.AddWithValue("@salt", saltBase64);
                        cmd.Parameters.AddWithValue("@id", employeeId);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user credentials: {ex.Message}");
                return false;
            }
        }



        private byte[] GenerateSalt()
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);
            return salt;
        }

        private string HashPassword(string password, byte[] salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] saltedPassword = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
                return Convert.ToBase64String(sha256.ComputeHash(saltedPassword));
            }
        }
    }
}
