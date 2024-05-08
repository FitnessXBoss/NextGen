using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NextGen.src.Services;
using NextGen.src.UI.Models;
using Npgsql;
using NextGen.src.Services.Security;
using NextGen.src.Components.Common.Utils;

namespace NextGen.src.UI.ViewModels
{
    public class EmployeeViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Employee> _employees = new ObservableCollection<Employee>();
        public ObservableCollection<Employee> Employees
        {
            get => _employees;
            private set
            {
                _employees = value;
                OnPropertyChanged();
            }
        }

        private DatabaseService _databaseService;

        public EmployeeViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;

            LoadData();
        }

        private Employee MaskSensitiveData(Employee employee, bool canViewPersonal)
        {
            if (canViewPersonal) return employee;  // Если пользователь имеет права, возвращаем объект без изменений

            var properties = typeof(Employee).GetProperties();
            foreach (var prop in properties)
            {
                if (Attribute.IsDefined(prop, typeof(SensitiveDataAttribute)))
                {
                    if (prop.PropertyType == typeof(string)) // Если свойство является строкой
                    {
                        prop.SetValue(employee, "****");
                    }
                    else if (prop.PropertyType == typeof(bool)) // Если свойство является булевым
                    {
                        prop.SetValue(employee, false); // Присвоить логическое значение false или true в зависимости от логики приложения
                    }
                    // Добавьте дополнительные условия для других типов данных, если это необходимо
                }
            }
            return employee;
        }



        private async void LoadData()
        {
            var tempList = new List<Employee>();
            bool canViewPersonal = UserSessionService.Instance.CurrentUser?.HasPermission("can_view_employees") ?? false;

            using (var conn = _databaseService.GetConnection())
            {
                var cmd = new NpgsqlCommand("SELECT * FROM employees_view", conn);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var employee = new Employee
                        {
                            EmployeeId = (int)reader["employee_id"],
                            FirstName = reader["first_name"] as string ?? string.Empty,
                            LastName = reader["last_name"] as string ?? string.Empty,
                            Position = reader["position"] as string ?? string.Empty,
                            Phone = reader["phone"] as string ?? string.Empty,
                            Email = reader["email"] as string ?? string.Empty,
                            RoleName = reader["role_name"] as string ?? string.Empty,
                            PhotoUrl = reader["photo_url"] as string ?? string.Empty,


                            PensionInsuranceNumber = reader["pension_insurance_number"] as string ?? string.Empty,
                            MilitaryIssuedBy = reader["military_issued_by"] as string ?? string.Empty,
                            MilitaryDutyStatus = reader["military_duty_status"] as string ?? string.Empty,
                            Gender = reader["gender"] as string ?? string.Empty,
                            PassportIssuedBy = reader["passport_issued_by"] as string ?? string.Empty,
                            PhoneNumber = reader["phone_number"] as string ?? string.Empty,
                            PassportSeriesNumber = reader["passport_series_number"] as string ?? string.Empty,
                            ConsentToProcessPersonalData = (bool)reader["consent_to_process_personal_data"],
                            CriminalRecord = (bool)reader["criminal_record"],
                            Profession = reader["profession"] as string ?? string.Empty,
                            PassportIssueDate = reader["passport_issue_date"] as string ?? string.Empty,                         
                            MilitaryIssueDate = reader["military_issue_date"] as string ?? string.Empty,
                            RegistrationPlace = reader["registration_place"] as string ?? string.Empty,
                            HealthInsuranceNumber = reader["health_insurance_number"] as string ?? string.Empty,
                            OtherData = reader["other_data"] as string ?? string.Empty,
                            MilitaryIdNumber = reader["military_id_number"] as string ?? string.Empty,
                            MaritalStatus = reader["marital_status"] as string ?? string.Empty,
                            ResidenceAddress = reader["residence_address"] as string ?? string.Empty,
                            DraftCardNumber = reader["draft_card_number"] as string ?? string.Empty,
                            DraftRegistrationDate = reader["draft_registration_date"] as string ?? string.Empty,
                            EducationLevel = reader["education_level"] as string ?? string.Empty,
                            TaxNumber = reader["tax_number"] as string ?? string.Empty,
                            TaxType = reader["tax_type"] as string ?? string.Empty,
                            Inn = reader["inn"] as string ?? string.Empty, 
                            Snils = reader["snils"] as string ?? string.Empty, 
                            Oms = reader["oms"] as string ?? string.Empty
                        };

                        tempList.Add(MaskSensitiveData(employee, canViewPersonal)); // Применяем маскировку к каждому сотруднику
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                Employees.Clear();
                foreach (var employee in tempList)
                {
                    Employees.Add(employee);
                }
            });

        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
