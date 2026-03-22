using System.ComponentModel.DataAnnotations;

namespace SocietyApp.Models
{
    public class Society
    {
        public int Id { get; set; }

        public Salutation Salutation { get; set; } = Salutation.Mr;

        [Required, MaxLength(50)]
        public string Name { get; set; } = "";

        [Required, MaxLength(100)]
        public string RegistrationNumber { get; set; } = "";

        [Required, MaxLength(500)]
        public string Address { get; set; } = "";

        // Unique code used for login (e.g. "GREENPARK2024")
        [Required, MaxLength(50)]
        public string SocietyCode { get; set; } = "";

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Member> Members { get; set; } = new List<Member>();
        public ICollection<Particular> Particulars { get; set; } = new List<Particular>();
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    }
}
