using NextGen.src.Data.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGen.src.UI.Models
{
    public static class TempDataStore
    {
        public static CarDetails CarDetails { get; set; }
        public static Customer SelectedCustomer { get; set; }
    }
}
