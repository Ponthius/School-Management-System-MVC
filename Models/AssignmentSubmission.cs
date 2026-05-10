using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        [ForeignKey("AssignmentId")]
        public Assignment? Assignment { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [MaxLength(260)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(600)]
        public string Notes { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Submitted";

        public decimal? Score { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.Now;
    }
}
