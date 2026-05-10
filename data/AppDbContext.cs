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
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<ExamTerm> ExamTerms { get; set; }
        public DbSet<ExamSchedule> ExamSchedules { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<FeeInvoice> FeeInvoices { get; set; }
        public DbSet<AcademicCalendarEvent> AcademicCalendarEvents { get; set; }
        public DbSet<Notice> Notices { get; set; }
        public DbSet<LearningMaterial> LearningMaterials { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<SchoolSetting> SchoolSettings { get; set; }
        public DbSet<HelpTicket> HelpTickets { get; set; }

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

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.FeeInvoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.FeeInvoiceId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<FeeInvoice>()
                .Property(i => i.AmountDue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FeeInvoice>()
                .Property(i => i.AmountPaid)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FeeInvoice>()
                .Property(i => i.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<FeeInvoice>()
                .HasOne(i => i.Student)
                .WithMany()
                .HasForeignKey(i => i.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttendanceRecord>()
                .HasIndex(a => new { a.StudentId, a.AttendanceDate })
                .IsUnique();

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(a => a.Class)
                .WithMany()
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AttendanceRecord>()
                .HasOne(a => a.MarkedByTeacher)
                .WithMany()
                .HasForeignKey(a => a.MarkedByTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ExamSchedule>()
                .HasOne(e => e.ExamTerm)
                .WithMany(t => t.Schedules)
                .HasForeignKey(e => e.ExamTermId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamSchedule>()
                .HasOne(e => e.Class)
                .WithMany()
                .HasForeignKey(e => e.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamSchedule>()
                .HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamSchedule>()
                .HasOne(e => e.InvigilatorTeacher)
                .WithMany()
                .HasForeignKey(e => e.InvigilatorTeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<ExamResult>()
                .HasIndex(r => new { r.ExamTermId, r.StudentId, r.SubjectId })
                .IsUnique();

            modelBuilder.Entity<ExamResult>()
                .HasOne(r => r.ExamTerm)
                .WithMany(t => t.Results)
                .HasForeignKey(r => r.ExamTermId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamResult>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ExamResult>()
                .HasOne(r => r.Subject)
                .WithMany()
                .HasForeignKey(r => r.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LearningMaterial>()
                .HasOne(m => m.Teacher)
                .WithMany()
                .HasForeignKey(m => m.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LearningMaterial>()
                .HasOne(m => m.Class)
                .WithMany()
                .HasForeignKey(m => m.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<LearningMaterial>()
                .HasOne(m => m.Subject)
                .WithMany()
                .HasForeignKey(m => m.SubjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Assignment>()
                .Property(a => a.MaxScore)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Teacher)
                .WithMany()
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Class)
                .WithMany()
                .HasForeignKey(a => a.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Subject)
                .WithMany()
                .HasForeignKey(a => a.SubjectId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AssignmentSubmission>()
                .Property(s => s.Score)
                .HasPrecision(5, 2);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasIndex(s => new { s.AssignmentId, s.StudentId })
                .IsUnique();

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SchoolSetting>()
                .HasIndex(s => s.Key)
                .IsUnique();

            modelBuilder.Entity<HelpTicket>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.PrimaryClass)
                .WithMany()
                .HasForeignKey(t => t.PrimaryClassId)
                .OnDelete(DeleteBehavior.SetNull);

            // Classes and subjects are intentionally not seeded.
            // Admins should create them from Classes & Subjects.
        }
    }
}
