using InternshipManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace InternshipManagement.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<InternshipPosition> InternshipPositions { get; set; }
        public DbSet<Application> Applications { get; set; }
    }
}