using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Xceed.Words.NET;

namespace NextGen.src.Services.Document
{
    public class DocumentGenerator
    {
        private readonly TemplateService _templateService;

        public DocumentGenerator()
        {
            _templateService = new TemplateService();
        }

        public List<string> GenerateDocument(string templateName, string outputPath, Dictionary<string, string> placeholders)
        {
            try
            {
                Debug.WriteLine($"Fetching template content for: {templateName}");
                byte[] templateContent = _templateService.GetTemplate(templateName);

                if (templateContent == null || templateContent.Length == 0)
                {
                    throw new Exception($"Template '{templateName}' is empty or not found in the database.");
                }

                Debug.WriteLine("Template content fetched successfully.");

                // Убедимся, что выходная директория существует
                string outputDir = Path.GetDirectoryName(outputPath);
                Debug.WriteLine($"Output directory: {outputDir}");
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                    Debug.WriteLine($"Created output directory: {outputDir}");
                }

                // Проверка на пустые значения
                List<string> emptyFields = placeholders.Where(p => string.IsNullOrEmpty(p.Value)).Select(p => p.Key).ToList();
                if (emptyFields.Count > 0)
                {
                    Debug.WriteLine($"Empty fields found: {string.Join(", ", emptyFields)}");
                    return TranslateFieldNames(emptyFields); // Возвращаем список пустых полей с русскими названиями
                }

                // Сохраняем шаблон во временный файл
                Debug.WriteLine($"Saving template content to temporary file: {outputPath}");
                File.WriteAllBytes(outputPath, templateContent);
                Debug.WriteLine($"Template saved to temporary file: {outputPath}");

                // Открываем документ и заменяем плейсхолдеры
                using (DocX document = DocX.Load(outputPath))
                {
                    foreach (var placeholder in placeholders)
                    {
                        string newValue = placeholder.Value ?? string.Empty;
                        Debug.WriteLine($"Replacing placeholder {placeholder.Key} with {newValue}");
                        document.ReplaceText(placeholder.Key, newValue);
                    }

                    // Сохраняем измененный документ в файл
                    Debug.WriteLine($"Saving document to {outputPath}");
                    document.SaveAs(outputPath);
                }

                Debug.WriteLine("Document generation completed successfully.");
                return new List<string>(); // Возвращаем пустой список, если все поля заполнены
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during document generation: {ex.Message}\nStack Trace: {ex.StackTrace}");
                throw;
            }
        }

        private List<string> TranslateFieldNames(List<string> fieldNames)
        {
            var translatedFields = new Dictionary<string, string>
            {
                { "{{Название компании}}", "Название компании" },
                { "{{Адрес компании}}", "Адрес компании" },
                { "{{Телефон компании}}", "Телефон компании" },
                { "{{Email компании}}", "Email компании" },
                { "{{Сайт компании}}", "Сайт компании" },
                { "{{Регистрационный номер компании}}", "Регистрационный номер компании" },
                { "{{ФИО директора}}", "ФИО директора" },
                { "{{Должность директора}}", "Должность директора" },
                { "{{Доверенность}}", "Доверенность" },
                { "{{ФИО сотрудника}}", "ФИО сотрудника" },
                { "{{Должность сотрудника}}", "Должность сотрудника" },
                { "{{ФИО покупателя}}", "ФИО покупателя" },
                { "{{Номер паспорта покупателя}}", "Номер паспорта покупателя" },
                { "{{Дата выдачи паспорта}}", "Дата выдачи паспорта" },
                { "{{Кем выдан паспорт}}", "Кем выдан паспорт" },
                { "{{Адрес покупателя}}", "Адрес покупателя" },
                { "{{Телефон покупателя}}", "Телефон покупателя" },
                { "{{Тип ТС}}", "Тип ТС" },
                { "{{Модель ТС}}", "Модель ТС" },
                { "{{Год выпуска ТС}}", "Год выпуска ТС" },
                { "{{Цвет ТС}}", "Цвет ТС" },
                { "{{Мощность двигателя}}", "Мощность двигателя" },
                { "{{VIN}}", "VIN" },
                { "{{Цена}}", "Цена" },
                { "{{Дата составления}}", "Дата составления" },
                { "{{Город}}", "Город" }
            };

            return fieldNames.Select(f => translatedFields.ContainsKey(f) ? translatedFields[f] : f).ToList();
        }
    }
}
