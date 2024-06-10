using Npgsql;
using System;
using System.Configuration;
using NextGen.src.Data.Database.Models;
using NextGen.src.UI.Models.NextGen.src.Data.Database.Models;

namespace NextGen.src.Services
{
    public class OrganizationService
    {
        private readonly string connectionString;

        public OrganizationService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public Organization GetOrganization()
        {
            Organization organization = null;
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT * FROM organizations LIMIT 1", connection); 
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        organization = new Organization
                        {
                            OrganizationId = reader.GetInt32(reader.GetOrdinal("organization_id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Address = reader.GetString(reader.GetOrdinal("address")),
                            Phone = reader.GetString(reader.GetOrdinal("phone")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            Website = reader.GetString(reader.GetOrdinal("website")),
                            RegistrationNumber = reader.GetString(reader.GetOrdinal("registration_number")),
                            DateEstablished = reader.GetDateTime(reader.GetOrdinal("date_established")),
                            INN = reader.GetString(reader.GetOrdinal("inn")),
                            KPP = reader.GetString(reader.GetOrdinal("kpp")),
                            OGRN = reader.GetString(reader.GetOrdinal("ogrn")),
                            OKPO = reader.GetString(reader.GetOrdinal("okpo")),
                            BankDetails = reader.GetString(reader.GetOrdinal("bank_details")),
                            DirectorFullName = reader.GetString(reader.GetOrdinal("director_full_name")),
                            DirectorTitle = reader.GetString(reader.GetOrdinal("director_title")),
                            PowerOfAttorney = reader.GetString(reader.GetOrdinal("power_of_attorney")),
                            City = reader.GetString(reader.GetOrdinal("city"))
                        };
                    }
                }
            }
            return organization;
        }


        public void UpdateOrganization(Organization organization)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand(
                    "UPDATE organizations SET name = @name, address = @address, phone = @phone, email = @e mail, website = @website, registration_number = @registration_number, date_established = @date_established, inn = @inn, kpp = @kpp, ogrn = @ogrn, okpo = @okpo, bank_details = @bank_details, director_full_name = @director_full_name, director_title = @director_title, power_of_attorney = @power_of_attorney WHERE organization_id = @organization_id",
                    connection);
                cmd.Parameters.AddWithValue("organization_id", organization.OrganizationId);
                cmd.Parameters.AddWithValue("name", organization.Name);
                cmd.Parameters.AddWithValue("address", organization.Address);
                cmd.Parameters.AddWithValue("phone", organization.Phone ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("email", organization.Email ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("website", organization.Website ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("registration_number", organization.RegistrationNumber);
                cmd.Parameters.AddWithValue("date_established", organization.DateEstablished);
                cmd.Parameters.AddWithValue("inn", organization.INN ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("kpp", organization.KPP ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("ogrn", organization.OGRN ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("okpo", organization.OKPO ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("bank_details", organization.BankDetails ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("director_full_name", organization.DirectorFullName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("director_title", organization.DirectorTitle ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("power_of_attorney", organization.PowerOfAttorney ?? (object)DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
