using jira_lite.Models;
using Microsoft.EntityFrameworkCore;

namespace jira_lite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Issue> Issues => Set<Issue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Key).IsRequired().HasMaxLength(10);
        });

        modelBuilder.Entity<Issue>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.Title).IsRequired().HasMaxLength(500);
            e.HasOne(i => i.Project)
             .WithMany(p => p.Issues)
             .HasForeignKey(i => i.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
