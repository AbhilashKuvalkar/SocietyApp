using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Services;

namespace SocietyApp.Controllers
{
    [Authorize]
    public class ReportController : AppController
    {
        private readonly ReportService _reportService;
        public ReportController(ReportService rs) => _reportService = rs;

        public async Task<IActionResult> Collection(string? year, int? quarter)
        {
            year ??= $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";
            var summary = await _reportService.GetCollectionSummaryAsync(GetSocietyId().GetValueOrDefault(), year, quarter);
            ViewBag.Year = year;
            ViewBag.Quarter = quarter;
            return View(summary);
        }

        public async Task<IActionResult> Defaulters(string? year, int? quarter)
        {
            year ??= $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";
            quarter ??= ((DateTime.Now.Month - 1) / 3) + 1;
            var defaulters = await _reportService.GetDefaultersAsync(GetSocietyId().GetValueOrDefault(), year, quarter.Value);
            ViewBag.Year = year;
            ViewBag.Quarter = quarter;
            return View(defaulters);
        }
    }
}
