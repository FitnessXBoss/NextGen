using Newtonsoft.Json;


namespace NextGen.src.Services.Security
{
    public class UserAuthData
    {
        public string? Username { get; set; }
        public int UserId { get; set; }
        public int EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{LastName} {FirstName} {MiddleName}";
        public string? PhotoUrl { get; set; }
        public string? RoleName { get; set; }
        public string? Permissions { get; set; }

        public bool HasPermission(string permission)
        {
            if (string.IsNullOrEmpty(Permissions))  // Проверка на null и пустую строку перед десериализацией
            {
                return false;
            }

            try
            {
                var permissionsDict = JsonConvert.DeserializeObject<Dictionary<string, bool>>(Permissions);
                if (permissionsDict != null)  // Проверка на null после десериализации
                {
                    return permissionsDict.TryGetValue(permission, out bool hasPerm) && hasPerm;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine("Error parsing JSON: " + ex.Message);  // Логирование ошибки десериализации
            }
            return false;
        }
    }
}
