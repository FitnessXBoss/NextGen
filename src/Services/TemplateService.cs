using Npgsql;
using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xceed.Words.NET;

namespace NextGen.src.Services
{
    public class TemplateService
    {
        private readonly string connectionString;

        public TemplateService()
        {
            connectionString = ConfigurationManager.ConnectionStrings["SecurityData"].ConnectionString;
        }

        public byte[] GetTemplate(string templateName)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("SELECT template_content FROM document_templates WHERE template_name = @templateName", connection);
                cmd.Parameters.AddWithValue("@templateName", templateName);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader["template_content"] as byte[];
                    }
                }
            }

            throw new Exception($"Template '{templateName}' not found");
        }

        public void SaveTemplate(string templateName, byte[] templateContent)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("INSERT INTO document_templates (template_name, template_content) VALUES (@templateName, @templateContent)", connection);
                cmd.Parameters.AddWithValue("@templateName", templateName);
                cmd.Parameters.AddWithValue("@templateContent", templateContent);
                cmd.ExecuteNonQuery();
            }
        }
    }
}