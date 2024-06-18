namespace NextGen.src.Models
{
    public class PaymentNotification
    {
        public string Comment { get; set; }
        public string Amount { get; set; }
        public string Sender { get; set; }
        public int CarId { get; set; } // Добавьте это свойство
    }
}
