using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class ExamSchedule
    {
        public int Id { get; set; }

        public int ExamTermId { get; set; }
        [ForeignKey("ExamTermId")]
        public ExamTerm? ExamTerm { get; set; }

        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class? Class { get; set; }

        public int SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        public int? InvigilatorTeacherId { get; set; }
        [ForeignKey("InvigilatorTeacherId")]
        public Teacher? InvigilatorTeacher { get; set; }

        [Required, MaxLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        [MaxLength(300)]
        public string Notes { get; set; } = string.Empty;
    }
}
