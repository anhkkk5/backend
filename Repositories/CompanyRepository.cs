using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories
{
    public class CompanyRepository : IRepository<Company>
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Company>> GetAllAsync()
        {
            return await _context.Companies.ToListAsync();
        }

        public async Task<Company> GetByIdAsync(int id)
        {
            return await _context.Companies.FindAsync(id);
        }

        public async Task CreateAsync(Company entity)
        {
            _context.Companies.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Company entity)
        {
            _context.Companies.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Companies.FindAsync(id);
            if (entity != null)
            {
                _context.Companies.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}