namespace MyJob.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options)
        {
        }

        
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Recruiter> Recruiters { get; set; }

        public DbSet<CV> CVs { get; set; }
        public DbSet<Job> Jobs { get; set; } 
        public DbSet<Applicant> Applicants { get; set; }
    }
}