using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NextGen.src.UI.Models
{
    public class PaymentResult
    {
        public decimal Amount { get; set; }
        public decimal AmountInRub { get; set; }
        public string Sender { get; set; }
        public decimal TonToRubRate { get; set; }
    }

}
