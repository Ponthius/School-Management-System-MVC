using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public string SubjectSpecialization { get; set; } = string.Empty;

        // Comma-separated subject names e.g. "Mathematics,Physics"
        public string AssignedSubjects { get; set; } = string.Empty;

        public int? PrimaryClassId { get; set; }
        [ForeignKey("PrimaryClassId")]
        public Class? PrimaryClass { get; set; }

        public string PhotoPath { get; set; } = "/images/default-avatar.png";
        public string Status { get; set; } = "Active"; // Active / Inactive
        public DateTime JoinedDate { get; set; } = DateTime.Now;
    }
}