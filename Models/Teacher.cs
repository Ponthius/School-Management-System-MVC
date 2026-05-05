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

        public string SubjectSpecialization { get; set; } = string.Empty;
        public DateTime JoinedDate { get; set; } = DateTime.Now;
    }
}