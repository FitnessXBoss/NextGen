using Npgsql;
using System;
using System.Security.Cryptography;
using System.Text;

namespace NextGen.src.Services.Security
{
    public class AuthService : DatabaseService
    {
        public AuthService() : base() { }

        public bool AuthenticateUser(string username, string password)
        {
            using (var conn = GetConnection())
            {
                using (var cmd = new NpgsqlCommand("SELECT password_hash, salt FROM users WHERE username = @username", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var storedHash = reader.GetString(0);
                            var storedSalt = reader.GetString(1);

                            try
                            {
                                var saltBytes = Convert.FromBase64String(storedSalt);
                                var hashOfEnteredPassword = HashPassword(password, saltBytes);
                                return storedHash == hashOfEnteredPassword;
                            }
                            catch (FormatException fe)
                            {
                                // Логирование ошибки
                                Console.WriteLine("Ошибка декодирования Base-64: " + fe.Message);
                                return false;
                            }
                        }
                    }
                }
            }
            return false;
        }



        public bool RegisterUser(string username, string password)
        {
            using (var conn = GetConnection())
            {
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