namespace InternshipManagement.Models
{
    public class Company
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Contact { get; set; }
        public string Description { get; set; }
    }
}