using System.ComponentModel.DataAnnotations;

namespace SocietyApp.Models
{
    public class Member
    {
        public int Id { get; set; }

        public int SocietyId { get; set; }

        public Salutation Salutation { get; set; } = Salutation.Mr;

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        [Required, MaxLength(20)]
        public string FlatNumber { get; set; } = "";

        public TenantType TenantType { get; set; } = TenantType.Permanent;

        [MaxLength(15)]
        public string? Phone { get; set; }

        [MaxLength(200)]
        public string? Email { get; set; }

        // For sublet tracking — if temporary tenant, who is the owner?
        [MaxLength(200)]
        public string? OwnerName { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? MoveInDate { get; set; }

        public DateTime? MoveOutDate { get; set; }

        // Navigation
        public Society? Society { get; set; }
        public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    }
}
