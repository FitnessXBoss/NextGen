using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGen.src.UI.Models
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
    }

}
