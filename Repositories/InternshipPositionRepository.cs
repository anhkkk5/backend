using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories
{
    public class InternshipPositionRepository : IRepository<InternshipPosition>
    {
        private readonly AppDbContext _context;

        public InternshipPositionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<InternshipPosition>> GetAllAsync()
        {
            return await _context.InternshipPositions.ToListAsync();
        }

        public async Task<InternshipPosition> GetByIdAsync(int id)
        {
            return await _context.InternshipPositions.FindAsync(id);
        }

        public async Task CreateAsync(InternshipPosition entity)
        {
            _context.InternshipPositions.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(InternshipPosition entity)
        {
            _context.InternshipPositions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.InternshipPositions.FindAsync(id);
            if (entity != null)
            {
                _context.InternshipPositions.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}