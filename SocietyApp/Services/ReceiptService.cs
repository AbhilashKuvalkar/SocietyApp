using SocietyApp.Data;
using SocietyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyApp.Services
{
    public class ReceiptService
    {
        private readonly AppDbContext _db;
        public ReceiptService(AppDbContext db) => _db = db;

        public Task<List<Receipt>> GetBySocietyAsync(int societyId, string? year = null, int? quarter = null)
        {
            var q = _db.Receipts
                .Include(r => r.Society)
                .Include(r => r.Member)
                .Include(r => r.LineItems).ThenInclude(l => l.Particular)
                .Where(r => r.SocietyId == societyId);
            if (year != null) q = q.Where(r => r.FinancialYear == year);
            if (quarter != null) q = q.Where(r => r.Quarter == quarter);
            return q.OrderByDescending(r => r.ReceiptDate).ToListAsync();
        }

        public Task<Receipt?> GetByIdAsync(int id) =>
            _db.Receipts
               .Include(r => r.Society)
               .Include(r => r.Member)
               .Include(r => r.LineItems).ThenInclude(l => l.Particular)
               .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<Receipt> CreateAsync(Receipt receipt, List<ReceiptLineItem> lineItems)
        {
            receipt.ReceiptNumber = await GenerateReceiptNumberAsync(receipt.SocietyId);
            receipt.TotalAmount = lineItems.Sum(l => l.Amount);
            // Assign via navigation property — EF inserts both Receipt and LineItems
            // in one SaveChangesAsync, avoiding the double-insert caused by adding
            // line items separately after the receipt is already tracked.
            receipt.LineItems = lineItems;
            _db.Receipts.Add(receipt);
            await _db.SaveChangesAsync();
            return receipt;
        }

        public async Task DeleteAsync(int id)
        {
            var r = await _db.Receipts.FindAsync(id);
            if (r != null) { _db.Receipts.Remove(r); await _db.SaveChangesAsync(); }
        }

        private async Task<string> GenerateReceiptNumberAsync(int societyId)
        {
            var society = await _db.Societies.FindAsync(societyId);
            var year = DateTime.Now.Year.ToString()[2..];
            var count = await _db.Receipts.CountAsync(r => r.SocietyId == societyId) + 1;
            var prefix = (society?.SocietyCode ?? "SOC")[..Math.Min(3, (society?.SocietyCode ?? "SOC").Length)].ToUpper();
            return $"{prefix}-{year}-{count:D4}";
        }

        // Helper: convert amount to words (Indian number system)
        public static string AmountToWords(decimal amount)
        {
            var units = new[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
                "Seventeen", "Eighteen", "Nineteen" };
            var tens = new[] { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            long n = (long)amount;
            if (n == 0) return "Zero";

            string words = "";
            if (n / 10000000 > 0) { words += AmountInWords((int)(n / 10000000), units, tens) + " Crore "; n %= 10000000; }
            if (n / 100000 > 0) { words += AmountInWords((int)(n / 100000), units, tens) + " Lakh "; n %= 100000; }
            if (n / 1000 > 0) { words += AmountInWords((int)(n / 1000), units, tens) + " Thousand "; n %= 1000; }
            if (n / 100 > 0) { words += AmountInWords((int)(n / 100), units, tens) + " Hundred "; n %= 100; }
            if (n > 0) { words += AmountInWords((int)n, units, tens); }

            return words.Trim() + " Only";
        }

        private static string AmountInWords(int n, string[] units, string[] tens)
        {
            if (n < 20) return units[n];
            return tens[n / 10] + (n % 10 > 0 ? " " + units[n % 10] : "");
        }
    }
}
