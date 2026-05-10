using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models
{
    public class SchoolSetting
    {
        public int Id { get; set; }

        [Required, MaxLength(80)]
        public string Key { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Value { get; set; } = string.Empty;

        [Required, MaxLength(40)]
        public string Group { get; set; } = "General";

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
