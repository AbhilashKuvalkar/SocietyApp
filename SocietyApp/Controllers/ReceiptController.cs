using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocietyApp.Models;
using SocietyApp.Services;
using System.Security.Claims;

namespace SocietyApp.Controllers
{
    [Authorize]
    public class ReceiptController : AppController
    {
        private readonly ReceiptService _receiptService;
        private readonly MemberService _memberService;
        private readonly ParticularService _particularService;
        private readonly PdfService _pdfService;

        public ReceiptController(ReceiptService rs, MemberService ms, ParticularService ps, PdfService pdf)
        { _receiptService = rs; _memberService = ms; _particularService = ps; _pdfService = pdf; }

        public async Task<IActionResult> Index(string? year, int? quarter)
        {
            ViewBag.Year = year;
            ViewBag.Quarter = quarter;
            int societyId = GetSocietyId().GetValueOrDefault();
            return View(await _receiptService.GetBySocietyAsync(societyId, year, quarter));
        }

        [HttpGet]
        public async Task<IActionResult> BulkPrint(string year, int quarter)
        {
            var receipts = await _receiptService.GetBySocietyAsync(GetSocietyId() ?? 0, year, quarter);
            ViewBag.Year = year;
            ViewBag.Quarter = quarter;
            return View(receipts);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var societyId = GetSocietyId().GetValueOrDefault();
            ViewBag.Members = await _memberService.GetBySocietyAsync(societyId);
            ViewBag.Particulars = await _particularService.GetBySocietyAsync(societyId);
            return View(new Receipt { SocietyId = societyId, ReceiptDate = DateTime.Today });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Receipt model, List<ReceiptLineItemInput> lineItems)
        {
            var items = (lineItems ?? new())
                .Select(l => new ReceiptLineItem { ParticularId = l.ParticularId, Amount = l.Amount })
                .ToList();

            model.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var receipt = await _receiptService.CreateAsync(model, items);
            TempData["Success"] = $"Receipt {receipt.ReceiptNumber} created.";
            return RedirectToAction(nameof(Print), new { id = receipt.Id });
        }

        public async Task<IActionResult> Print(int id)
        {
            var receipt = await _receiptService.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        public async Task<IActionResult> DownloadPdf(int id)
        {
            var receipt = await _receiptService.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            var pdf = _pdfService.GenerateReceipt(receipt);
            return File(pdf, "application/pdf", $"Receipt-{receipt.ReceiptNumber}.pdf");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _receiptService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
