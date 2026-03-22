using System.ComponentModel.DataAnnotations;

namespace SocietyApp.Models
{
    public class Particular
    {
        public int Id { get; set; }
        public int SocietyId { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";  // e.g. "Municipal Taxes"

        public decimal DefaultAmount { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Society? Society { get; set; }
        public ICollection<ReceiptLineItem> LineItems { get; set; } = new List<ReceiptLineItem>();
    }
}
