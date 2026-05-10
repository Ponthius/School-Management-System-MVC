using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // ── Guard helpers ──────────────────────────────────────
        private bool IsLoggedIn() =>
            HttpContext.Session.GetString("UserRole") != null;

        private IActionResult? GuardRole(string role)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");
            if (HttpContext.Session.GetString("UserRole") != role)
                return RedirectToAction("Login", "Account");
            return null;
        }

        // ── Admin Dashboard ────────────────────────────────────
        public IActionResult Admin()
        {
            var guard = GuardRole("Admin");
            if (guard != null) return guard;

            // Counts
            int totalStudents = _context.Students.Count();
            int totalTeachers = _context.Teachers.Count();
            int totalClasses = _context.Classes.Count();
            decimal feesCollected = _context.Payments
                .Where(p => p.Status == "Complete")
                .Sum(p => (decimal?)p.Amount) ?? 0;

            // Room occupancy — avg students vs capacity
            int occupancy = 0;
            if (totalClasses > 0)
            {
                var classes = _context.Classes
                    .Include(c => c.Students)
                    .ToList();
                double avg = classes.Average(c =>
                    c.Capacity > 0
                        ? (double)c.Students.Count / c.Capacity * 100
                        : 0);
                occupancy = (int)Math.Round(avg);
            }

            // Staff availability — teachers with a class assigned / total
            int staffAvail = totalTeachers > 0
                ? (int)Math.Round(
                    (double)_context.Classes.Count(c => c.TeacherId != null)
                    / totalTeachers * 100)
                : 0;

            var today = DateTime.Today;
            int todaysMarkedAttendance = _context.AttendanceRecords.Count(a => a.AttendanceDate == today);
            int todaysPresentAttendance = _context.AttendanceRecords.Count(a => a.AttendanceDate == today && a.Status == "Present");
            int dailyAttendancePercent = todaysMarkedAttendance > 0
                ? (int)Math.Round((double)todaysPresentAttendance / todaysMarkedAttendance * 100)
                : 0;

            // Recent activity — last 10 logs
            var recentActivity = _context.ActivityLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(10)
                .ToList();

            var vm = new AdminDashboardViewModel
            {
                TotalStudents = totalStudents,
                TotalTeachers = totalTeachers,
                TotalClasses = totalClasses,
                TotalFeesCollected = feesCollected,
                GradeLevels = _context.Classes.Select(c => c.Name).Distinct().Count(),
                StudentRatio = totalTeachers > 0
                                            ? $"1:{totalStudents / totalTeachers}"
                                            : "N/A",
                RoomOccupancyPercent = occupancy,
                MonthlyGoalPercent = feesCollected > 0
                                            ? Math.Min((int)(feesCollected / 20000000 * 100), 100)
                                            : 0,
                DailyAttendancePercent = dailyAttendancePercent,
                StaffAvailabilityPercent = staffAvail,
                RecentActivity = recentActivity,
                UpcomingEvents = new List<UpcomingEvent>
                {
                    new() { Month="OCT", Day="12", Title="Annual Sports Meet",
                            Location="Main Stadium", Time="09:00 AM" },
                    new() { Month="OCT", Day="15", Title="PTA Meeting.",
                            Location="Virtual Session", Time="02:00 PM" }
                }
            };

            return View(vm);
        }

        // ── Teacher Dashboard (stub — Day 5) ───────────────────
        public IActionResult Teacher()
        {
            var guard = GuardRole("Teacher");
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            var teacher = _context.Teachers
                .Include(t => t.PrimaryClass)
                .FirstOrDefault(t => t.UserId == userId);
            if (teacher == null) return NotFound();

            var assignedSubjectNames = teacher.AssignedSubjects
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var classQuery = _context.Classes
                .Include(c => c.Students)
                .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                .AsQueryable();

            var classes = classQuery
                .Where(c => c.TeacherId == teacher.Id || c.Id == teacher.PrimaryClassId)
                .ToList();

            if (assignedSubjectNames.Any())
            {
                var subjectClasses = classQuery
                    .Where(c => c.ClassSubjects.Any(cs => cs.Subject != null && assignedSubjectNames.Contains(cs.Subject.Name)))
                    .ToList();
                classes = classes.Concat(subjectClasses)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .ToList();
            }

            var classIds = classes.Select(c => c.Id).ToList();
            var today = DateTime.Today;
            var schedules = _context.ExamSchedules
                .Include(s => s.Class)
                .Include(s => s.Subject)
                .Where(s => classIds.Contains(s.ClassId) && s.ExamDate == today)
                .OrderBy(s => s.StartTime)
                .ToList();

            var vm = new TeacherDashboardViewModel
            {
                Teacher = teacher,
                AssignedClassCount = classes.Count,
                StudentCount = classes.SelectMany(c => c.Students).Select(s => s.Id).Distinct().Count(),
                PendingMarks = _context.ExamSchedules.Count(s => classIds.Contains(s.ClassId) && s.ExamDate >= today),
                TodaySchedules = schedules,
                Announcements = _context.AcademicCalendarEvents
                    .Where(e => e.Audience == "All" || e.Audience == "Teacher")
                    .OrderBy(e => e.EventDate)
                    .Take(4)
                    .ToList()
            };

            return View(vm);
        }

        // ── Student Dashboard (stub — Day 5) ───────────────────
        public IActionResult Student()
        {
            var guard = GuardRole("Student");
            if (guard != null) return guard;

            var userId = HttpContext.Session.GetInt32("UserId");
            var student = _context.Students
                .Include(s => s.Class)
                .Include(s => s.User)
                .FirstOrDefault(s => s.UserId == userId);
            if (student == null) return NotFound();

            var subjectCount = _context.ClassSubjects.Count(cs => cs.ClassId == student.ClassId);
            if (subjectCount == 0) subjectCount = _context.Subjects.Count();

            var results = _context.ExamResults
                .Include(r => r.Subject)
                .Include(r => r.ExamTerm)
                .Where(r => r.StudentId == student.Id && r.IsPublished)
                .OrderByDescending(r => r.UpdatedAt)
                .Take(6)
                .ToList();
            var average = results.Any(r => r.Score.HasValue)
                ? Math.Round(results.Where(r => r.Score.HasValue).Average(r => r.Score!.Value), 1)
                : 0;

            var vm = new StudentDashboardViewModel
            {
                Student = student,
                FeeBalance = _context.FeeInvoices.Where(i => i.StudentId == student.Id).Sum(i => (decimal?)i.Balance) ?? 0,
                SubjectCount = subjectCount,
                AverageScore = average,
                RecentResults = results,
                Notices = _context.Notices
                    .Where(n => n.Audience == "All" || n.Audience == "Student")
                    .OrderByDescending(n => n.PublishDate)
                    .Take(4)
                    .ToList()
            };

            return View(vm);
        }
    }
}
