using SocietyApp.Data;
using SocietyApp.Models;
using Microsoft.EntityFrameworkCore;

namespace SocietyApp.Services
{
    public class MemberService
    {
        private readonly AppDbContext _db;
        public MemberService(AppDbContext db) => _db = db;

        public Task<List<Member>> GetBySocietyAsync(int societyId) =>
            _db.Members.Where(m => m.SocietyId == societyId && m.IsActive)
                       .OrderBy(m => m.FlatNumber)
                       .ToListAsync();

        public Task<Member?> GetByIdAsync(int id) =>
            _db.Members.FindAsync(id).AsTask();

        public async Task<Member> CreateAsync(Member member)
        {
            _db.Members.Add(member);
            await _db.SaveChangesAsync();
            return member;
        }

        public async Task UpdateAsync(Member member)
        {
            _db.Members.Update(member);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var m = await _db.Members.FindAsync(id);
            if (m != null) { m.IsActive = false; await _db.SaveChangesAsync(); }
        }
    }
}
