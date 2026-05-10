using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _context;

        public AttendanceController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? Guard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role != "Admin") return RedirectToAction("Admin", "Dashboard");
            return null;
        }

        private void LogActivity(string action, string status = "Complete")
        {
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action = action,
                Status = status
            });
        }

        public async Task<IActionResult> Index(int? classId, DateTime? date, string? search, string? status)
        {
            var guard = Guard(); if (guard != null) return guard;

            var classes = await _context.Classes
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (!classId.HasValue && classes.Any())
                classId = classes.First().Id;

            var selectedDate = (date ?? DateTime.Today).Date;
            var normalizedSearch = search?.Trim() ?? string.Empty;
            var normalizedStatus = status?.Trim() ?? string.Empty;

            var studentQuery = _context.Students
                .Include(s => s.Class)
                .AsQueryable();

            if (classId.HasValue)
                studentQuery = studentQuery.Where(s => s.ClassId == classId.Value);

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
                studentQuery = studentQuery.Where(s => s.FullName.Contains(normalizedSearch));

            var students = await studentQuery
                .OrderBy(s => s.FullName)
                .ToListAsync();

            var studentIds = students.Select(s => s.Id).ToList();

            var records = await _context.AttendanceRecords
                .Include(a => a.MarkedByTeacher)
                .Where(a => a.AttendanceDate == selectedDate && studentIds.Contains(a.StudentId))
                .ToListAsync();

            var rows = students.Select(student =>
            {
                var record = records.FirstOrDefault(a => a.StudentId == student.Id);
                var markedBy = record == null
                    ? string.Empty
                    : record.Source == "Teacher" && record.MarkedByTeacher != null
                        ? record.MarkedByTeacher.FullName
                        : string.IsNullOrWhiteSpace(record.MarkedByName)
                            ? "Admin"
                            : record.MarkedByName;

                return new AttendanceStudentRowViewModel
                {
                    StudentId = student.Id,
                    ClassId = student.ClassId,
                    StudentName = student.FullName,
                    ClassName = student.Class?.Name ?? "Unassigned",
                    PhotoPath = student.PhotoPath,
                    Status = record?.Status ?? "Not Marked",
                    Notes = record?.Notes ?? string.Empty,
                    MarkedByName = markedBy,
                    MarkedBySource = record?.Source ?? string.Empty,
                    MarkedAt = record?.MarkedAt
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(normalizedStatus))
                rows = rows
                    .Where(r => string.Equals(r.Status, normalizedStatus, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var vm = new AttendanceDashboardViewModel
            {
                SelectedClassId = classId,
                SelectedDate = selectedDate,
                Search = normalizedSearch,
                Status = normalizedStatus,
                ClassOptions = classes
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Name,
                        Selected = classId == c.Id
                    })
                    .ToList(),
                Students = rows,
                TotalStudents = students.Count,
                PresentCount = rows.Count(r => r.Status == "Present"),
                AbsentCount = rows.Count(r => r.Status == "Absent"),
                LateCount = rows.Count(r => r.Status == "Late"),
                NotMarkedCount = rows.Count(r => r.Status == "Not Marked")
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveStatus(AttendanceStatusUpdateViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            var allowed = new[] { "Present", "Absent", "Late", "Excused" };
            if (!allowed.Contains(vm.Status))
            {
                TempData["Error"] = "Select a valid attendance status.";
                return RedirectToAction(nameof(Index), new { classId = vm.SelectedClassId, date = vm.AttendanceDate, search = vm.Search });
            }

            var student = await _context.Students
                .Include(s => s.Class)
                .FirstOrDefaultAsync(s => s.Id == vm.StudentId);

            if (student == null) return NotFound();

            var attendanceDate = vm.AttendanceDate.Date;
            var record = await _context.AttendanceRecords
                .FirstOrDefaultAsync(a => a.StudentId == vm.StudentId && a.AttendanceDate == attendanceDate);

            if (record == null)
            {
                record = new AttendanceRecord
                {
                    StudentId = student.Id,
                    ClassId = student.ClassId,
                    AttendanceDate = attendanceDate,
                    MarkedAt = DateTime.Now
                };
                _context.AttendanceRecords.Add(record);
            }

            record.ClassId = student.ClassId;
            record.Status = vm.Status;
            record.Notes = vm.Notes?.Trim() ?? string.Empty;
            record.Source = "Admin";
            record.MarkedByTeacherId = null;
            record.MarkedByName = HttpContext.Session.GetString("Username") ?? "Admin";
            record.UpdatedAt = DateTime.Now;

            LogActivity($"Marked {student.FullName} as {vm.Status} for {attendanceDate:dd MMM yyyy}", "Verified");
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Attendance updated for {student.FullName}.";
            return RedirectToAction(nameof(Index), new
            {
                classId = vm.SelectedClassId ?? student.ClassId,
                date = attendanceDate.ToString("yyyy-MM-dd"),
                search = vm.Search
            });
        }
    }
}
