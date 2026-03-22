using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Services;

namespace SocietyApp.Controllers
{
    [Authorize]
    public class HomeController : AppController
    {
        private readonly ReportService _reportService;
        public HomeController(ReportService rs) => _reportService = rs;

        public async Task<IActionResult> Index()
        {
            var societyId = GetSocietyId().GetValueOrDefault();
            var year = $"{DateTime.Now.Year - 1}-{DateTime.Now.Year}";
            var summary = await _reportService.GetCollectionSummaryAsync(societyId, year);
            return View(summary);
        }
    }
}
