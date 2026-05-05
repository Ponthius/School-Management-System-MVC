using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }

        [MaxLength(100)]
        public string UserName { get; set; } = string.Empty;

        public string UserRole { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Action { get; set; } = string.Empty;

        public string Status { get; set; } = "Verified"; // Verified / Complete / Action Req.

        public DateTime Timestamp { get; set; } = DateTime.Now;

        public int? RelatedUserId { get; set; }
    }
}