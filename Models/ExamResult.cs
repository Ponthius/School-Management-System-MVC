using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class ExamResult
    {
        public int Id { get; set; }

        public int ExamTermId { get; set; }
        [ForeignKey("ExamTermId")]
        public ExamTerm? ExamTerm { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? Score { get; set; }

        [MaxLength(300)]
        public string TeacherComments { get; set; } = string.Empty;

        [MaxLength(100)]
        public string RecordedByName { get; set; } = string.Empty;

        public bool IsPublished { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
