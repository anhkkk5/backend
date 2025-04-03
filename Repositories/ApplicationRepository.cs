using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories
{
    public class ApplicationRepository : IRepository<Application>
    {
        private readonly AppDbContext _context;

        public ApplicationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Application>> GetAllAsync()
        {
            return await _context.Applications.ToListAsync();
        }

        public async Task<Application> GetByIdAsync(int id)
        {
            return await _context.Applications.FindAsync(id);
        }

        public async Task CreateAsync(Application entity)
        {
            _context.Applications.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Application entity)
        {
            _context.Applications.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Applications.FindAsync(id);
            if (entity != null)
            {
                _context.Applications.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}