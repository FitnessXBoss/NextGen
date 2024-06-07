namespace NextGen.src.Data.Database.Models
{
    public class Customer
    {
        public string FullName => $"{FirstName} {LastName}";

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int CreatedBy { get; set; } // Добавлено поле для отслеживания создателя записи
    }
}
