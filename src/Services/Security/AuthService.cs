using Npgsql;
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OtpNet; // Добавьте пакет Otp.Net для работы с TOTP

namespace NextGen.src.Services.Security
{
    public class AuthService : DatabaseService
    {
        public AuthService() : base() { }

        public async Task<(UserAuthData?, string?)> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                using (var conn = await GetConnectionAsync())
                {
                    using (var cmd = new NpgsqlCommand("SELECT user_id, username, password_hash, salt, employee_id FROM users WHERE username = @username", conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        using (var reader = await ExecuteReaderWithRetryAsync(cmd))
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
                Debug.WriteLine($"Database connection error: {ex.Message}");
                return (null, "Ошибка подключения к базе данных!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Authentication error: {ex.Message}");
            }
            return (null, "Неверное имя пользователя или пароль!");
        }


        public async Task SaveSecretKeyAsync(string username, string secretKey)
        {
            try
            {
                using (var conn = await GetConnectionAsync())
                {
                    var cmd = new NpgsqlCommand("UPDATE users SET totp_secret = @secretKey, two_factor_enabled = true WHERE username = @username", conn);
                    cmd.Parameters.AddWithValue("@secretKey", secretKey);
                    cmd.Parameters.AddWithValue("@username", username);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error saving secret key: {ex.Message}");
            }
        }

        public async Task<(bool, string?)> ValidateTwoFactorCodeAsync(int userId, string code)
        {
            try
            {
                using (var conn = await GetConnectionAsync())
                {
                    using (var cmd = new NpgsqlCommand("SELECT totp_secret FROM users WHERE user_id = @userId", conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        using (var reader = await ExecuteReaderWithRetryAsync(cmd))
                        {
                            if (reader.Read())
                            {
                                var totpSecret = reader.GetString(0);
                                var totp = new Totp(Base32Encoding.ToBytes(totpSecret));
                                if (totp.VerifyTotp(code, out long _))
                                {
                                    return (true, null);
                                }
                                else
                                {
                                    return (false, "Неверный код двухфакторной аутентификации.");
                                }
                            }
                            else
                            {
                                return (false, "Пользователь не найден.");
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                Debug.WriteLine($"Database connection error: {ex.Message}");
                return (false, "Ошибка подключения к базе данных!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Authentication error: {ex.Message}");
                return (false, "Ошибка при проверке кода.");
            }
        }


        public async Task<bool> RegisterUserAsync(string username, string password)
        {
            try
            {
                using (var conn = await GetConnectionAsync())
                {
                    var cmd = new NpgsqlCommand("INSERT INTO users (username, password_hash, salt, totp_secret, two_factor_enabled) VALUES (@username, @hash, @salt, @totpSecret, @twoFactorEnabled)", conn);
                    var salt = GenerateSalt();
                    var hash = HashPassword(password, salt);
                    var saltBase64 = Convert.ToBase64String(salt);
                    var totpSecret = KeyGeneration.GenerateRandomKey(20); // Генерация случайного секретного ключа для TOTP
                    var totpSecretBase32 = Base32Encoding.ToString(totpSecret);

                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@hash", hash);
                    cmd.Parameters.AddWithValue("@salt", saltBase64);
                    cmd.Parameters.AddWithValue("@totpSecret", totpSecretBase32);
                    cmd.Parameters.AddWithValue("@twoFactorEnabled", true);

                    int result = await cmd.ExecuteNonQueryAsync();
                    return result > 0;
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибок базы данных или других исключений
                Debug.WriteLine($"Registration error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserCredentialsAsync(int employeeId, string newPassword)
        {
            try
            {
                using (var conn = await GetConnectionAsync())
                {
                    byte[] newSalt = GenerateSalt();
                    string newPasswordHash = HashPassword(newPassword, newSalt);
                    string saltBase64 = Convert.ToBase64String(newSalt);

                    using (var cmd = new NpgsqlCommand("UPDATE users SET password_hash = @password, salt = @salt WHERE employee_id = @id", conn))
                    {
                        cmd.Parameters.AddWithValue("@password", newPasswordHash);
                        cmd.Parameters.AddWithValue("@salt", saltBase64);
                        cmd.Parameters.AddWithValue("@id", employeeId);

                        int result = await cmd.ExecuteNonQueryAsync();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error updating user credentials: {ex.Message}");
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
