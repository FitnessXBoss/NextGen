using System;
using NextGen.src.Components.Common.Utils;

namespace NextGen.src.UI.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Position { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? RoleName { get; set; }

        [SensitiveData]
        public string? PensionInsuranceNumber { get; set; }
        [SensitiveData]
        public string? PhoneNumber { get; set; }
        [SensitiveData]
        public string? MilitaryIssuedBy { get; set; }
        [SensitiveData] public string? MilitaryDutyStatus { get; set; }
        public string? Gender { get; set; }
        [SensitiveData]
        public string? PassportIssuedBy { get; set; }
        [SensitiveData]
        public string? PassportSeriesNumber { get; set; }
        [SensitiveData]
        public bool ConsentToProcessPersonalData { get; set; }
        [SensitiveData]
        public bool CriminalRecord { get; set; }
        [SensitiveData]
        public string? Profession { get; set; }
        [SensitiveData]
        public string? PassportIssueDate { get; set; }
        [SensitiveData]
        public string? MilitaryIssueDate { get; set; }
        [SensitiveData]
        public string? RegistrationPlace { get; set; }
        [SensitiveData]
        public string? HealthInsuranceNumber { get; set; }
        [SensitiveData]
        public string? OtherData { get; set; }
        [SensitiveData]
        public string? MilitaryIdNumber { get; set; }
        [SensitiveData]
        public string? PhotoUrl { get; set; }
        [SensitiveData]
        public string? MaritalStatus { get; set; }
        [SensitiveData]
        public string? ResidenceAddress { get; set; }
        [SensitiveData]
        public string? DraftCardNumber { get; set; }
        [SensitiveData]
        public string? DraftRegistrationDate { get; set; }
        [SensitiveData]
        public string? EducationLevel { get; set; }
        [SensitiveData]
        public string? TaxNumber { get; set; }
        [SensitiveData]
        public string? TaxType { get; set; }
        [SensitiveData]
        public string? Inn { get; set; }
        [SensitiveData]
        public string? Snils { get; set; }
        [SensitiveData]
        public string? Oms { get; set; }
        [SensitiveData]
        public int? RoleId { get; set; } 

    }
}
