using jira_lite.Models;
using Microsoft.EntityFrameworkCore;
using Task = jira_lite.Models.Task;

namespace jira_lite.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserProjectRole> UserProjectRoles => Set<UserProjectRole>();
    public DbSet<WorkflowStatus> WorkflowStatuses => Set<WorkflowStatus>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Epic> Epics => Set<Epic>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<Subtask> Subtasks => Set<Subtask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // --- User ---
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.PasswordHash).IsRequired();
        });

        // --- Role ---
        modelBuilder.Entity<Role>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired().HasMaxLength(50);
        });

        // --- UserProjectRole (composite PK) ---
        modelBuilder.Entity<UserProjectRole>(e =>
        {
            e.HasKey(upr => new { upr.UserId, upr.ProjectId, upr.RoleId });
            e.HasOne(upr => upr.User).WithMany(u => u.UserProjectRoles).HasForeignKey(upr => upr.UserId);
            e.HasOne(upr => upr.Project).WithMany(p => p.UserProjectRoles).HasForeignKey(upr => upr.ProjectId);
            e.HasOne(upr => upr.Role).WithMany(r => r.UserProjectRoles).HasForeignKey(upr => upr.RoleId);
        });

        // --- WorkflowStatus ---
        modelBuilder.Entity<WorkflowStatus>(e =>
        {
            e.HasKey(ws => ws.Id);
            e.Property(ws => ws.Name).IsRequired().HasMaxLength(100);
            e.Property(ws => ws.Color).HasMaxLength(20);
        });

        // --- WorkflowTransition ---
        modelBuilder.Entity<WorkflowTransition>(e =>
        {
            e.HasKey(wt => wt.Id);
            e.Property(wt => wt.EntityType).IsRequired().HasMaxLength(50);
            e.HasOne(wt => wt.FromStatus)
             .WithMany(ws => ws.FromTransitions)
             .HasForeignKey(wt => wt.FromStatusId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(wt => wt.ToStatus)
             .WithMany(ws => ws.ToTransitions)
             .HasForeignKey(wt => wt.ToStatusId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // --- Project ---
        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Key).IsRequired().HasMaxLength(10);
            e.HasIndex(p => p.Key).IsUnique();
            e.HasOne(p => p.CreatedBy).WithMany(u => u.CreatedProjects)
             .HasForeignKey(p => p.CreatedById).OnDelete(DeleteBehavior.Restrict);
        });

        // --- Epic ---
        modelBuilder.Entity<Epic>(e =>
        {
            e.HasKey(ep => ep.Id);
            e.Property(ep => ep.Title).IsRequired().HasMaxLength(500);
            e.Property(ep => ep.Priority).HasMaxLength(20);
            e.HasOne(ep => ep.Project).WithMany(p => p.Epics)
             .HasForeignKey(ep => ep.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ep => ep.Status).WithMany()
             .HasForeignKey(ep => ep.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ep => ep.CreatedBy).WithMany()
             .HasForeignKey(ep => ep.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(ep => ep.Assignee).WithMany()
             .HasForeignKey(ep => ep.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        });

        // --- Story ---
        modelBuilder.Entity<Story>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Title).IsRequired().HasMaxLength(500);
            e.Property(s => s.Priority).HasMaxLength(20);
            e.HasOne(s => s.Epic).WithMany(ep => ep.Stories)
             .HasForeignKey(s => s.EpicId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Status).WithMany()
             .HasForeignKey(s => s.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.CreatedBy).WithMany()
             .HasForeignKey(s => s.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Assignee).WithMany()
             .HasForeignKey(s => s.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        });

        // --- Task ---
        modelBuilder.Entity<Task>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).IsRequired().HasMaxLength(500);
            e.Property(t => t.Priority).HasMaxLength(20);
            e.Property(t => t.EstimatedHours).HasPrecision(8, 2);
            e.Property(t => t.LoggedHours).HasPrecision(8, 2);
            e.HasOne(t => t.Story).WithMany(s => s.Tasks)
             .HasForeignKey(t => t.StoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.Status).WithMany()
             .HasForeignKey(t => t.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.CreatedBy).WithMany()
             .HasForeignKey(t => t.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(t => t.Assignee).WithMany()
             .HasForeignKey(t => t.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        });

        // --- Subtask ---
        modelBuilder.Entity<Subtask>(e =>
        {
            e.HasKey(st => st.Id);
            e.Property(st => st.Title).IsRequired().HasMaxLength(500);
            e.Property(st => st.Priority).HasMaxLength(20);
            e.HasOne(st => st.Task).WithMany(t => t.Subtasks)
             .HasForeignKey(st => st.TaskId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(st => st.Status).WithMany()
             .HasForeignKey(st => st.StatusId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(st => st.CreatedBy).WithMany()
             .HasForeignKey(st => st.CreatedById).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(st => st.Assignee).WithMany()
             .HasForeignKey(st => st.AssigneeId).OnDelete(DeleteBehavior.SetNull);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // --- Roles ---
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Project Admin", Description = "Full access to the project" },
            new Role { Id = 2, Name = "Lead",          Description = "Can manage epics and stories" },
            new Role { Id = 3, Name = "Member",        Description = "Can work on assigned tasks" },
            new Role { Id = 4, Name = "Viewer",        Description = "Read-only access" }
        );

        // --- WorkflowStatuses ---
        modelBuilder.Entity<WorkflowStatus>().HasData(
            new WorkflowStatus { Id = 1, Name = "Todo",        Color = "#e2e8f0", Order = 1 },
            new WorkflowStatus { Id = 2, Name = "In Progress", Color = "#3b82f6", Order = 2 },
            new WorkflowStatus { Id = 3, Name = "In Review",   Color = "#f59e0b", Order = 3 },
            new WorkflowStatus { Id = 4, Name = "Done",        Color = "#22c55e", Order = 4 }
        );

        // --- WorkflowTransitions ---
        // Applies to all entity types: Epic, Story, Task, Subtask
        var entityTypes = new[] { "Epic", "Story", "Task", "Subtask" };
        var transitions = new List<WorkflowTransition>();
        int transitionId = 1;

        // Allowed transitions per entity type
        var allowedTransitions = new[] {
            (From: 1, To: 2), // Todo        → In Progress
            (From: 2, To: 3), // In Progress → In Review
            (From: 3, To: 4), // In Review   → Done
            (From: 3, To: 2), // In Review   → In Progress (send back)
            (From: 2, To: 1)  // In Progress → Todo        (send back)
        };

        foreach (var entityType in entityTypes)
            foreach (var t in allowedTransitions)
                transitions.Add(new WorkflowTransition
                {
                    Id = transitionId++,
                    FromStatusId = t.From,
                    ToStatusId = t.To,
                    EntityType = entityType
                });

        modelBuilder.Entity<WorkflowTransition>().HasData(transitions);

        // --- Seed Admin User (password: Admin@123) ---
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FullName = "Admin",
                Email = "admin@jiralite.com",
                // BCrypt hash of "Admin@123"
                PasswordHash = "$2a$11$ow/8YBLWFpFMJwCBESHpEusFNJOHY8bFJmNrCpRwiqrjrRGRDDfgK",
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // --- Seed Sample Project ---
        modelBuilder.Entity<Project>().HasData(
            new Project
            {
                Id = 1,
                Name = "Jira Lite",
                Key = "JL",
                Description = "A lightweight Jira clone for learning purposes",
                CreatedById = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );

        // --- Assign Admin as Project Admin on sample project ---
        modelBuilder.Entity<UserProjectRole>().HasData(
            new UserProjectRole
            {
                UserId = 1,
                ProjectId = 1,
                RoleId = 1,
                AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}
