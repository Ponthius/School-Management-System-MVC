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
                DailyAttendancePercent = 94,   // placeholder — replace when attendance module is built
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
            return View();
        }

        // ── Student Dashboard (stub — Day 5) ───────────────────
        public IActionResult Student()
        {
            var guard = GuardRole("Student");
            if (guard != null) return guard;
            return View();
        }
    }
}