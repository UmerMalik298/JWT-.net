using JWT.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWT.Data
{
    public class ApplicationDbContext : DbContext
    {
      public  ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> User { get; set; }   
        public DbSet<UserProfile> UserProfile { get; set; }
    }
}
