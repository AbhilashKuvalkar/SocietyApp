using System.ComponentModel.DataAnnotations;

namespace SocietyApp.Models
{
    public class Receipt
    {
        public int Id { get; set; }
        public int SocietyId { get; set; }
        public int MemberId { get; set; }

        // Auto-generated: SOC001-2526-0001
        [MaxLength(50)]
        public string ReceiptNumber { get; set; } = "";

        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;

        // Period e.g. "2025-2026" + quarter Q1-Q4
        [MaxLength(20)]
        public string FinancialYear { get; set; } = "";  // "2025-2026"
        public int Quarter { get; set; }                 // 1-4

        public decimal TotalAmount { get; set; }
        public PaymentMode PaymentMode { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Paid;

        [MaxLength(100)]
        public string? PaymentReference { get; set; }  // Cheque/NEFT ref number

        [MaxLength(500)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedByUserId { get; set; } = "";

        // Navigation
        public Society? Society { get; set; }
        public Member? Member { get; set; }
        public ICollection<ReceiptLineItem> LineItems { get; set; } = new List<ReceiptLineItem>();

    }
}
