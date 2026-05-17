using Microsoft.EntityFrameworkCore;
using TaskTracker.Models;

namespace TaskTracker.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<UserTask> Tasks { get; set; }

        public AppDbContext() { }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string dbPath = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "TaskTracker", "tasks.db");

                var directory = System.IO.Path.GetDirectoryName(dbPath);
                if (directory != null && !System.IO.Directory.Exists(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                }

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserTask>()
                .HasOne(t => t.Sprint)
                .WithMany(s => s.Tasks)
                .HasForeignKey(t => t.SprintId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}