namespace NextGen.src.Data.Database.Models
{
    public class Customer
    {
        public string FullName => $"{LastName} {FirstName} {MiddleName}";

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string PassportNumber { get; set; }
        public DateTime PassportIssueDate { get; set; }
        public string PassportIssuer { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public int CreatedBy { get; set; }
        public int CarId { get; set; }
    }
}