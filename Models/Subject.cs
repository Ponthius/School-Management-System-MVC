using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Department { get; set; } = string.Empty;

        public ICollection<ClassSubject> ClassSubjects { get; set; } = new List<ClassSubject>();
    }
}
