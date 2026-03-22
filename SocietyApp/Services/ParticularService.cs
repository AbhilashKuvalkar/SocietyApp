using SocietyApp.Data;
using SocietyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyApp.Services
{
    public class ParticularService
    {
        private readonly AppDbContext _db;
        public ParticularService(AppDbContext db) => _db = db;

        public Task<List<Particular>> GetBySocietyAsync(int societyId) =>
            _db.Particulars.Where(p => p.SocietyId == societyId && p.IsActive)
                           .OrderBy(p => p.DisplayOrder)
                           .ToListAsync();

        public Task<Particular?> GetByIdAsync(int id) =>
            _db.Particulars.FindAsync(id).AsTask();

        public async Task<Particular> CreateAsync(Particular particular)
        {
            _db.Particulars.Add(particular);
            await _db.SaveChangesAsync();
            return particular;
        }

        public async Task UpdateAsync(Particular particular)
        {
            _db.Particulars.Update(particular);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var p = await _db.Particulars.FindAsync(id);
            if (p != null) { p.IsActive = false; await _db.SaveChangesAsync(); }
        }
    }
}
