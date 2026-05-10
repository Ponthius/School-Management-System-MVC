using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class LearningMaterial
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        [ForeignKey("TeacherId")]
        public Teacher? Teacher { get; set; }

        public int? ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class? Class { get; set; }

        public int? SubjectId { get; set; }
        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }

        [Required, MaxLength(140)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(600)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(260)]
        public string FilePath { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string MaterialType { get; set; } = "Document";

        public DateTime UploadedAt { get; set; } = DateTime.Now;
    }
}
