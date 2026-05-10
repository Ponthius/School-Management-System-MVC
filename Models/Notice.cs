using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models
{
    public class Notice
    {
        public int Id { get; set; }

        [Required, MaxLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Body { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Audience { get; set; } = "All";

        [Required, MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        public DateTime PublishDate { get; set; } = DateTime.Today;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
