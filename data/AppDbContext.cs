using Microsoft.EntityFrameworkCore;
using School_Management_System.Models;

namespace School_Management_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<ClassSubject> ClassSubjects { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed default admin user
            modelBuilder.Entity<User>().HasData(new User
            {
                Id = 1,
                Username = "admin",
                Password = "admin123",
                Role = "Admin"
            });

            // Prevent cascade delete chains
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.HeadedClasses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ClassSubject>()
                .HasKey(cs => new { cs.ClassId, cs.SubjectId });

            modelBuilder.Entity<ClassSubject>()
                .HasOne(cs => cs.Class)
                .WithMany(c => c.ClassSubjects)
                .HasForeignKey(cs => cs.ClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassSubject>()
                .HasOne(cs => cs.Subject)
                .WithMany(s => s.ClassSubjects)
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Class)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

                modelBuilder.Entity<Payment>()
        .Property(p => p.Amount)
        .HasPrecision(18, 2);

    modelBuilder.Entity<Payment>()
        .Property(p => p.Balance)
        .HasPrecision(18, 2);

            // Seed default classes
            modelBuilder.Entity<Class>().HasData(
                new Class { Id = 1, Name = "S1-A", Capacity = 40 },
                new Class { Id = 2, Name = "S1-B", Capacity = 40 },
                new Class { Id = 3, Name = "S2-A", Capacity = 40 },
                new Class { Id = 4, Name = "S2-B", Capacity = 40 },
                new Class { Id = 5, Name = "S3-A", Capacity = 40 },
                new Class { Id = 6, Name = "S3-B", Capacity = 40 },
                new Class { Id = 7, Name = "P1", Capacity = 45 },
                new Class { Id = 8, Name = "P2", Capacity = 45 },
                new Class { Id = 9, Name = "P3", Capacity = 45 },
                new Class { Id = 10, Name = "P4", Capacity = 45 },
                new Class { Id = 11, Name = "P5", Capacity = 45 },
                new Class { Id = 12, Name = "P6", Capacity = 45 },
                new Class { Id = 13, Name = "P7", Capacity = 45 }
            );

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.PrimaryClass)
                .WithMany()
                .HasForeignKey(t => t.PrimaryClassId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Subject>().HasData(
                SubjectCatalog.SeedSubjects.Select(subject => new Subject
                {
                    Id = subject.Id,
                    Code = subject.Code,
                    Name = subject.Name,
                    Department = subject.Department
                })
            );
        }
    }
}
