using SocietyApp.Data;
using SocietyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyApp.Services
{
    public class SocietyService
    {
        private readonly AppDbContext _db;
        public SocietyService(AppDbContext db) => _db = db;

        public Task<List<Society>> GetAllAsync() =>
            _db.Societies.Where(s => s.IsActive).ToListAsync();

        public Task<Society?> GetByIdAsync(int id) =>
            _db.Societies.FindAsync(id).AsTask();

        public Task<Society?> GetByCodeAsync(string code) =>
            _db.Societies.FirstOrDefaultAsync(s => s.SocietyCode == code);

        public async Task<Society> CreateAsync(Society society)
        {
            _db.Societies.Add(society);
            await _db.SaveChangesAsync();

            // Seed default particulars — defined once in AppDbContext.DefaultParticulars.
            // SeedDefaultParticularsAsync skips any names that already exist for this society.
            //await _db.SeedDefaultParticularsAsync(society.Id);
            return society;
        }

        public async Task UpdateAsync(Society society)
        {
            _db.Societies.Update(society);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var s = await _db.Societies.FindAsync(id);
            if (s != null) { s.IsActive = false; await _db.SaveChangesAsync(); }
        }
    }
}
