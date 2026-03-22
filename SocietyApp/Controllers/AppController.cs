using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace SocietyApp.Controllers
{
    public abstract class AppController : Controller
    {
        // Returns null for super admin (no society claim). Controllers must handle this.
        protected int? GetSocietyId()
        {
            var claim = User.FindFirstValue("SocietyId");
            return int.TryParse(claim, out var id) ? id : null;
        }

        protected bool IsSuperAdmin => GetSocietyId() == null;
    }
}
