using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services;

namespace SocietyApp.Controllers
{
    [Authorize]
    public class MemberController : AppController
    {
        private readonly MemberService _service;
        private readonly SocietyService _societyService;

        public MemberController(MemberService s, SocietyService ss)
        { _service = s; _societyService = ss; }

        public async Task<IActionResult> Index()
        {
            int societyId = GetSocietyId().GetValueOrDefault();
            List<Member> model = await _service.GetBySocietyAsync(societyId);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateSocietiesAsync();
            int societyId = GetSocietyId().GetValueOrDefault();
            return View(new Member { SocietyId = societyId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member model)
        {
            if (!ModelState.IsValid) { await PopulateSocietiesAsync(); return View(model); }
            await _service.CreateAsync(model);
            TempData["Success"] = "Member added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var m = await _service.GetByIdAsync(id);
            if (m == null) return NotFound();
            await PopulateSocietiesAsync();
            return View(m);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Member model)
        {
            if (!ModelState.IsValid) { await PopulateSocietiesAsync(); return View(model); }
            await _service.UpdateAsync(model);
            TempData["Success"] = "Member updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Passes societies list to view; for regular users the dropdown will be
        // pre-selected and disabled so they cannot change their own society.
        private async Task PopulateSocietiesAsync() =>
            ViewBag.Societies = await _societyService.GetAllAsync();
    }
}
