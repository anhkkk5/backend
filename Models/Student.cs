namespace InternshipManagement.Models
{
    public class Student
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Skills { get; set; } 
        public string CvUrl { get; set; }
    }
}