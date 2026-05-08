using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models.ViewModels
{
    public class TeacherCreateViewModel
    {
        // Login credentials
        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Teacher info
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        // Subjects typed as comma-separated or multi-select
        public List<string> SelectedSubjects { get; set; } = new();

        public int? PrimaryClassId { get; set; }

        public string Status { get; set; } = "Active";

        public IFormFile? Photo { get; set; }
    }

    public class TeacherEditViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        public List<string> SelectedSubjects { get; set; } = new();

        public int? PrimaryClassId { get; set; }

        public string Status { get; set; } = "Active";

        public IFormFile? Photo { get; set; }
        public string ExistingPhoto { get; set; } = string.Empty;
    }
}