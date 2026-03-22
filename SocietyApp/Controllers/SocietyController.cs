using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services;

namespace SocietyApp.Controllers
{
    [Authorize]
    public class SocietyController : AppController
    {
        private readonly SocietyService _service;
        public SocietyController(SocietyService s) => _service = s;

        public async Task<IActionResult> Index() => View(await _service.GetAllAsync());

        [HttpGet] public IActionResult Create() => View(new Society());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Society model)
        {
            if (!ModelState.IsValid) return View(model);
            await _service.CreateAsync(model);
            TempData["Success"] = "Society created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var society = await _service.GetByIdAsync(id);
            if (society == null) return NotFound();
            return View(society);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Society model)
        {
            if (!ModelState.IsValid) return View(model);
            await _service.UpdateAsync(model);
            TempData["Success"] = "Society updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            TempData["Success"] = "Society deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
