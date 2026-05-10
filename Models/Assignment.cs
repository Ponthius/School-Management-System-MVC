using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher? Teacher { get; set; }

        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class? Class { get; set; }

        public int? SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        [Required, MaxLength(140)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
        public decimal MaxScore { get; set; } = 100;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
