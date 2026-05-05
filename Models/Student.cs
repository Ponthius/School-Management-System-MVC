using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace School_Management_System.Models
{
    public class Student
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class? Class { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;
        public DateTime DOB { get; set; }
        public string PhotoPath { get; set; } = "/images/default-avatar.png";

        [MaxLength(100)]
        public string GuardianName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string GuardianContact { get; set; } = string.Empty;

        public string Status { get; set; } = "New";
        public string Term { get; set; } = string.Empty;
        public DateTime EnrolledDate { get; set; } = DateTime.Now;
    }
}