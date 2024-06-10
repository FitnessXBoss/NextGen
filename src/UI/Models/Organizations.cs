using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGen.src.UI.Models
{
    namespace NextGen.src.Data.Database.Models
    {
        public class Organization
        {
            public int OrganizationId { get; set; }
            public string Name { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public string Website { get; set; }
            public string RegistrationNumber { get; set; }
            public DateTime DateEstablished { get; set; }
            public string INN { get; set; }
            public string KPP { get; set; }
            public string OGRN { get; set; }
            public string OKPO { get; set; }
            public string BankDetails { get; set; }
            public string DirectorFullName { get; set; }
            public string DirectorTitle { get; set; }
            public string PowerOfAttorney { get; set; }
            public string City { get; set; } // Новое поле
        }
    }


}
