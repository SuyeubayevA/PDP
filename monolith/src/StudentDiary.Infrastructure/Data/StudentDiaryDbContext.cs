using Microsoft.EntityFrameworkCore;
using StudentDiary.Domain.Entities;

namespace StudentDiary.Infrastructure.Data;

public class StudentDiaryDbContext : DbContext
{
    public StudentDiaryDbContext(DbContextOptions<StudentDiaryDbContext> options) : base(options) { }
    public DbSet<Student> Students { get; set; } = null!;
    public DbSet<Grade> Grades { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<Student>()
            .Property(s => s.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Grade>()
            .HasKey(g => g.Id);
        modelBuilder.Entity<Grade>()
            .Property(g => g.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Student>()
            .HasMany(s => s.Grades)
            .WithOne()
            .HasForeignKey(g => g.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Outbox
        modelBuilder.Entity<OutboxMessage>().HasKey(o => o.Id);
        modelBuilder.Entity<OutboxMessage>()
            .Property(o => o.Id)
            .ValueGeneratedOnAdd();
    }
}
