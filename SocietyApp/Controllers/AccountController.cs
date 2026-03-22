using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services;

namespace SocietyApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SocietyService _societyService;

        public AccountController(UserManager<ApplicationUser> um, SignInManager<ApplicationUser> sm, SocietyService ss)
        { _userManager = um; _signInManager = sm; _societyService = ss; }

        [HttpGet] public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string? societyCode)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) { ModelState.AddModelError("", "Invalid credentials."); return View(); }

            if (user.SocietyId != null)
            {
                // User already has a society — validate the supplied code matches it.
                var society = await _societyService.GetByCodeAsync(societyCode ?? "");
                if (society == null || user.SocietyId != society.Id)
                { ModelState.AddModelError("", "Invalid credentials."); return View(); }
            }
            else if (!string.IsNullOrWhiteSpace(societyCode))
            {
                // User has no society yet (created by super admin without one).
                // Resolve the society from the supplied code and persist it to the user
                // so the claims factory can include it in the session cookie.
                var society = await _societyService.GetByCodeAsync(societyCode);
                if (society == null)
                { ModelState.AddModelError("", "Invalid society code."); return View(); }

                user.SocietyId = society.Id;
                await _userManager.UpdateAsync(user);
            }
            // else: super admin login — no society code supplied, none required.

            var result = await _signInManager.PasswordSignInAsync(user, password, false, false);
            if (!result.Succeeded) { ModelState.AddModelError("", "Invalid credentials."); return View(); }

            return RedirectToAction("Index", "Home");
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> Logout()
        { await _signInManager.SignOutAsync(); return RedirectToAction("Login"); }
    }
}
