using Npgsql;
using System;
using System.Configuration;

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
            var duplicateReason = CheckTemplateDuplicate(templateName, templateContent);
            if (!string.IsNullOrEmpty(duplicateReason))
            {
                throw new Exception(duplicateReason);
            }

            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var cmd = new NpgsqlCommand("INSERT INTO document_templates (template_name, template_content) VALUES (@templateName, @templateContent)", connection);
                cmd.Parameters.AddWithValue("@templateName", templateName);
                cmd.Parameters.AddWithValue("@templateContent", templateContent);
                cmd.ExecuteNonQuery();
            }
        }

        private string CheckTemplateDuplicate(string templateName, byte[] templateContent)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // Check by name
                var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM document_templates WHERE template_name = @templateName", connection);
                cmd.Parameters.AddWithValue("@templateName", templateName);
                var count = (long)cmd.ExecuteScalar();

                if (count > 0)
                {
                    return $"Шаблон с именем '{templateName}' уже существует.";
                }

                // Check by content
                cmd = new NpgsqlCommand("SELECT template_content FROM document_templates", connection);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var existingTemplateContent = reader["template_content"] as byte[];
                        if (existingTemplateContent != null && AreArraysEqual(existingTemplateContent, templateContent))
                        {
                            return "Шаблон с идентичным содержимым уже существует.";
                        }
                    }
                }
            }

            return null;
        }

        private bool AreArraysEqual(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }

            for (int i = 0; i < array1.Length; i++)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
