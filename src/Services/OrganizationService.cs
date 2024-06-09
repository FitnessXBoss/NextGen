using System.Collections.Generic;
using System.Configuration;
using Npgsql;
using NextGen.src.Data.Database.Models;
using NextGen.src.UI.Models;

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
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT * FROM organizations LIMIT 1", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Organization
                        {
                            OrganizationId = reader.GetInt32(reader.GetOrdinal("organization_id")),
                            Name = reader.GetString(reader.GetOrdinal("name")),
                            Address = reader.GetString(reader.GetOrdinal("address")),
                            Phone = reader.GetString(reader.GetOrdinal("phone")),
                            Email = reader.GetString(reader.GetOrdinal("email")),
                            Website = reader.GetString(reader.GetOrdinal("website")),
                            RegistrationNumber = reader.GetString(reader.GetOrdinal("registration_number")),
                            DateEstablished = reader.GetDateTime(reader.GetOrdinal("date_established"))
                        };
                    }
                }
            }
            return null;
        }
    }
}
