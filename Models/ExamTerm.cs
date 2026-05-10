using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models
{
    public class ExamTerm
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Upcoming";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<ExamSchedule> Schedules { get; set; } = new List<ExamSchedule>();
        public ICollection<ExamResult> Results { get; set; } = new List<ExamResult>();
    }
}
