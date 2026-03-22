using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services;

namespace SocietyApp.Controllers
{
    [Authorize]
    public class ParticularController : AppController
    {
        private readonly ParticularService _service;
        public ParticularController(ParticularService s) => _service = s;

        public async Task<IActionResult> Index() =>
            View(await _service.GetBySocietyAsync(base.GetSocietyId().GetValueOrDefault()));

        [HttpGet] public IActionResult Create() => View(new Particular { SocietyId = base.GetSocietyId().GetValueOrDefault() });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Particular model)
        {
            if (!ModelState.IsValid) return View(model);
            await _service.CreateAsync(model);
            TempData["Success"] = "Particular added.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _service.GetByIdAsync(id);
            if (p == null) return NotFound();
            return View(p);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Particular model)
        {
            if (!ModelState.IsValid) return View(model);
            await _service.UpdateAsync(model);
            TempData["Success"] = "Particular updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
