﻿namespace InternshipManagement.Models
{
    public class InternshipPosition
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int Slots { get; set; }
        public string Status { get; set; }
    }
}