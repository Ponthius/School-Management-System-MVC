using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class FeeInvoice
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required, MaxLength(80)]
        public string Term { get; set; } = string.Empty;

        public decimal AmountDue { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal Balance { get; set; }

        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
