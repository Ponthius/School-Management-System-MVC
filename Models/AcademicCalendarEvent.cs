using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models
{
    public class AcademicCalendarEvent
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public DateTime EventDate { get; set; } = DateTime.Today;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        [MaxLength(120)]
        public string Location { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Audience { get; set; } = "All";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
