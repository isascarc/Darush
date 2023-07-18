
using Microsoft.EntityFrameworkCore;
using MyJob.Entities;

namespace MyJob.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<CV> CVs { get; set; }
    }
}