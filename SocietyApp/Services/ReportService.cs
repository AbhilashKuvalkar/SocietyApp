using SocietyApp.Data;
using SocietyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyApp.Services
{
    public class CollectionSummary
    {
        public decimal TotalCollected { get; set; }
        public decimal TotalPending { get; set; }
        public int ReceiptCount { get; set; }
        public List<ParticularTotal> ParticularBreakdown { get; set; } = new();
    }

    public class ParticularTotal
    {
        public string Name { get; set; } = "";
        public decimal Total { get; set; }
    }

    public class ReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db) => _db = db;

        public async Task<CollectionSummary> GetCollectionSummaryAsync(int societyId, string year, int? quarter = null)
        {
            var q = _db.Receipts.Where(r => r.SocietyId == societyId && r.FinancialYear == year);
            if (quarter.HasValue) q = q.Where(r => r.Quarter == quarter);

            var receipts = await q.Include(r => r.LineItems).ThenInclude(l => l.Particular).ToListAsync();

            return new CollectionSummary
            {
                TotalCollected = receipts.Where(r => r.PaymentStatus == PaymentStatus.Paid).Sum(r => r.TotalAmount),
                TotalPending = receipts.Where(r => r.PaymentStatus != PaymentStatus.Paid).Sum(r => r.TotalAmount),
                ReceiptCount = receipts.Count,
                ParticularBreakdown = receipts
                    .SelectMany(r => r.LineItems)
                    .GroupBy(l => l.Particular?.Name ?? "Unknown")
                    .Select(g => new ParticularTotal { Name = g.Key, Total = g.Sum(l => l.Amount) })
                    .OrderByDescending(p => p.Total)
                    .ToList()
            };
        }

        public async Task<List<Member>> GetDefaultersAsync(int societyId, string year, int quarter)
        {
            var paidMemberIds = await _db.Receipts
                .Where(r => r.SocietyId == societyId && r.FinancialYear == year &&
                            r.Quarter == quarter && r.PaymentStatus == PaymentStatus.Paid)
                .Select(r => r.MemberId)
                .ToListAsync();

            return await _db.Members
                .Where(m => m.SocietyId == societyId && m.IsActive && !paidMemberIds.Contains(m.Id))
                .OrderBy(m => m.FlatNumber)
                .ToListAsync();
        }
    }
}
