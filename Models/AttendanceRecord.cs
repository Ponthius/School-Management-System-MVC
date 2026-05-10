using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class AttendanceRecord
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class? Class { get; set; }

        public int? MarkedByTeacherId { get; set; }
        [ForeignKey("MarkedByTeacherId")]
        public Teacher? MarkedByTeacher { get; set; }

        public DateTime AttendanceDate { get; set; }

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Present";

        [Required, MaxLength(20)]
        public string Source { get; set; } = "Admin";

        [MaxLength(100)]
        public string MarkedByName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Notes { get; set; } = string.Empty;

        public DateTime MarkedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
