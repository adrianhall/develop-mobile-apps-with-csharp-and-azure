using Chapter1.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace Chapter1.Service.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TaskItemDTO> TaskItems => Set<TaskItemDTO>();

        public async Task InitializeDatabaseAsync()
        {
            await Database.EnsureCreatedAsync();
        }
    }
}