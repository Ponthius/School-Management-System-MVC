using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace School_Management_System.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        public int? FeeInvoiceId { get; set; }
        [ForeignKey("FeeInvoiceId")]
        public FeeInvoice? FeeInvoice { get; set; }

        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public string Term { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public string Status { get; set; } = "Pending";
        public string ReceiptNumber { get; set; } = string.Empty;
        public string RecordedByName { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
