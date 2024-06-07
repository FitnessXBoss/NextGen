namespace NextGen.src.Data.Database.Models
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int CarId { get; set; }
        public int UserId { get; set; }
        public int CustomerId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal Amount { get; set; }
    }
}
