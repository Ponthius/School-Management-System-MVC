using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models.ViewModels
{
    public class StudentCreateViewModel
    {
        // Login credentials (admin creates these)
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Student info
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; } = DateTime.Now.AddYears(-10);

        [Required]
        public int ClassId { get; set; }

        [MaxLength(100)]
        public string GuardianName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string GuardianContact { get; set; } = string.Empty;

        public string Status { get; set; } = "New";
        public string Term { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }
    }

    public class StudentEditViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string Gender { get; set; } = string.Empty;

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public int ClassId { get; set; }

        [MaxLength(100)]
        public string GuardianName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string GuardianContact { get; set; } = string.Empty;

        public string Status { get; set; } = "New";
        public string Term { get; set; } = string.Empty;

        public IFormFile? Photo { get; set; }
        public string ExistingPhoto { get; set; } = string.Empty;
    }
}