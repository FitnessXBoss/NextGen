using System;

namespace NextGen.src.UI.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string? PensionInsuranceNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? MilitaryIssuedBy { get; set; }
        public string? MilitaryDutyStatus { get; set; }
        public string? Gender { get; set; }
        public string? PassportIssuedBy { get; set; }
        public string? Position { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PassportSeriesNumber { get; set; }
        public bool ConsentToProcessPersonalData { get; set; }
        public bool CriminalRecord { get; set; }
        public string? Profession { get; set; }
        public string? PassportIssueDate { get; set; } // Изменено на string?
        public string? Email { get; set; }
        public string? MilitaryIssueDate { get; set; } // Изменено на string?
        public string? RegistrationPlace { get; set; }
        public string? HealthInsuranceNumber { get; set; }
        public string? OtherData { get; set; }
        public string? MilitaryIdNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public string? MaritalStatus { get; set; }
        public string? ResidenceAddress { get; set; }
        public string? Phone { get; set; }
        public string? DraftCardNumber { get; set; }
        public string? DraftRegistrationDate { get; set; } // Изменено на string?
        public string? RoleName { get; set; }
        public string? EducationLevel { get; set; }
        public string? TaxNumber { get; set; }
        public string? TaxType { get; set; }
        public string? Inn { get; set; } // Добавлено
        public string? Snils { get; set; } // Добавлено
        public string? Oms { get; set; } // Добавлено
        public int? RoleId { get; set; } // Добавлено
    }
}
