using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SocietyApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int? SocietyId { get; set; }

        [MaxLength(50)]
        public string Role { get; set; } = "Secretary"; // Chairman | Secretary | Treasurer | Member

        public Society? Society { get; set; }
    }
}
