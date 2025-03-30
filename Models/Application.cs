namespace InternshipManagement.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int PositionId { get; set; }
        public int CompanyId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? InterviewDate { get; set; }
        public string InterviewTime { get; set; } = string.Empty;
        public string InterviewLocation { get; set; } = string.Empty;
    }
}