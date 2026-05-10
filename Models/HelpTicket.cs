using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class HelpTicket
    {
        public int Id { get; set; }

        public int? UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required, MaxLength(120)]
        public string Subject { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Open";

        [Required, MaxLength(20)]
        public string Priority { get; set; } = "Normal";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
