using Microsoft.EntityFrameworkCore;
using Models;

namespace DB
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        
        }

        public DbSet<UserModel> users { get; set; }

        public DbSet<EmplooyeModel> employees { get; set; }
    }
}