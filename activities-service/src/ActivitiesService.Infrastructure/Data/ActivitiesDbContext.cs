using Microsoft.EntityFrameworkCore;
using ActivitiesService.Domain.Entities;

namespace ActivitiesService.Infrastructure.Data;

public class ActivitiesDbContext : DbContext
{
    public ActivitiesDbContext(DbContextOptions<ActivitiesDbContext> options) : base(options) { }

    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<ActivityEnrollment> Enrollments => Set<ActivityEnrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>().HasKey(a => a.Id);
        modelBuilder.Entity<Activity>().Property(a => a.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<ActivityEnrollment>().HasKey(e => e.Id);
        modelBuilder.Entity<ActivityEnrollment>().Property(e => e.Id).ValueGeneratedOnAdd();

        modelBuilder.Entity<ActivityEnrollment>()
            .HasIndex(e => new { e.ActivityId, e.StudentId });
    }
}
