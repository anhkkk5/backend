using InternshipManagement.Data;
using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Repositories
{
    public class AccountRepository : IRepository<Account>
    {
        private readonly AppDbContext _context;

        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Account>> GetAllAsync()
        {
            return await _context.Accounts.ToListAsync();
        }

        public async Task<Account> GetByIdAsync(int id)
        {
            return await _context.Accounts.FindAsync(id);
        }

        public async Task CreateAsync(Account entity)
        {
            _context.Accounts.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account entity)
        {
            _context.Accounts.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Accounts.FindAsync(id);
            if (entity != null)
            {
                _context.Accounts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}