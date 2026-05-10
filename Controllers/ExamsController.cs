using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class ExamsController : Controller
    {
        private readonly AppDbContext _context;

        public ExamsController(AppDbContext context)
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

        public async Task<IActionResult> Index(string? search, string? status, int? termId)
        {
            var guard = Guard(); if (guard != null) return guard;

            var normalizedSearch = search?.Trim() ?? string.Empty;
            var normalizedStatus = status?.Trim() ?? string.Empty;

            var termQuery = _context.ExamTerms
                .Include(t => t.Schedules)
                    .ThenInclude(s => s.Class)
                        .ThenInclude(c => c!.Students)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
                termQuery = termQuery.Where(t => t.Name.Contains(normalizedSearch) || t.Description.Contains(normalizedSearch));

            if (!string.IsNullOrWhiteSpace(normalizedStatus))
                termQuery = termQuery.Where(t => t.Status == normalizedStatus);

            var terms = await termQuery
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            var schedulesQuery = _context.ExamSchedules
                .Include(s => s.ExamTerm)
                .Include(s => s.Class)
                .Include(s => s.Subject)
                .Include(s => s.InvigilatorTeacher)
                .AsQueryable();

            if (termId.HasValue)
                schedulesQuery = schedulesQuery.Where(s => s.ExamTermId == termId.Value);

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
                schedulesQuery = schedulesQuery.Where(s =>
                    s.ExamTerm!.Name.Contains(normalizedSearch) ||
                    s.Class!.Name.Contains(normalizedSearch) ||
                    s.Subject!.Name.Contains(normalizedSearch) ||
                    s.RoomNumber.Contains(normalizedSearch));

            var schedules = await schedulesQuery
                .OrderBy(s => s.ExamDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            var scores = await _context.ExamResults
                .Where(r => r.Score.HasValue)
                .Select(r => r.Score!.Value)
                .ToListAsync();

            var passed = scores.Count(s => s >= 50);

            var vm = new ExamManagementViewModel
            {
                Search = normalizedSearch,
                Status = normalizedStatus,
                SelectedTermId = termId,
                OverallAverage = scores.Any() ? Math.Round(scores.Average(), 1) : 0,
                PassingRate = scores.Any() ? Math.Round((decimal)passed / scores.Count * 100, 1) : 0,
                TotalParticipants = await _context.Students.CountAsync(),
                ActiveTermCount = await _context.ExamTerms.CountAsync(t => t.Status == "Active"),
                Terms = terms.Select(t => new ExamTermRowViewModel
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Status = t.Status,
                    ScheduleCount = t.Schedules.Count,
                    ParticipantCount = t.Schedules
                        .Where(s => s.Class != null)
                        .SelectMany(s => s.Class!.Students)
                        .Select(s => s.Id)
                        .Distinct()
                        .Count()
                }).ToList(),
                Schedules = schedules.Select(ToScheduleRow).ToList(),
                TermOptions = await _context.ExamTerms
                    .OrderByDescending(t => t.StartDate)
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name, Selected = termId == t.Id })
                    .ToListAsync(),
                ClassOptions = await _context.Classes
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                    .ToListAsync(),
                SubjectOptions = await _context.Subjects
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = $"{s.Code} - {s.Name}" })
                    .ToListAsync(),
                TeacherOptions = await _context.Teachers
                    .OrderBy(t => t.FullName)
                    .Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.FullName })
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveTerm(ExamTermUpsertViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (string.IsNullOrWhiteSpace(vm.Name) || vm.EndDate.Date < vm.StartDate.Date)
            {
                TempData["Error"] = "Check the exam term name and date range.";
                return RedirectToAction(nameof(Index));
            }

            var status = NormalizeTermStatus(vm.Status);

            if (vm.Id.HasValue)
            {
                var term = await _context.ExamTerms.FindAsync(vm.Id.Value);
                if (term == null) return NotFound();

                term.Name = vm.Name.Trim();
                term.Description = vm.Description?.Trim() ?? string.Empty;
                term.StartDate = vm.StartDate.Date;
                term.EndDate = vm.EndDate.Date;
                term.Status = status;

                LogActivity($"Updated exam term {term.Name}");
                TempData["Success"] = $"Updated exam term {term.Name}.";
            }
            else
            {
                var term = new ExamTerm
                {
                    Name = vm.Name.Trim(),
                    Description = vm.Description?.Trim() ?? string.Empty,
                    StartDate = vm.StartDate.Date,
                    EndDate = vm.EndDate.Date,
                    Status = status
                };
                _context.ExamTerms.Add(term);

                LogActivity($"Created exam term {term.Name}", "Verified");
                TempData["Success"] = $"Created exam term {term.Name}.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSchedule(ExamScheduleUpsertViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (vm.ExamTermId <= 0 || vm.ClassId <= 0 || vm.SubjectId <= 0 ||
                string.IsNullOrWhiteSpace(vm.RoomNumber) || vm.EndTime <= vm.StartTime)
            {
                TempData["Error"] = "Check the class, subject, room, and exam time.";
                return RedirectToAction(nameof(Index), new { termId = vm.ExamTermId });
            }

            if (vm.Id.HasValue)
            {
                var schedule = await _context.ExamSchedules.FindAsync(vm.Id.Value);
                if (schedule == null) return NotFound();

                ApplySchedule(schedule, vm);
                LogActivity("Updated an exam schedule");
                TempData["Success"] = "Exam schedule updated.";
            }
            else
            {
                var schedule = new ExamSchedule();
                ApplySchedule(schedule, vm);
                _context.ExamSchedules.Add(schedule);
                LogActivity("Added an exam schedule", "Verified");
                TempData["Success"] = "Exam schedule added.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { termId = vm.ExamTermId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSchedule(int id, int? termId)
        {
            var guard = Guard(); if (guard != null) return guard;

            var schedule = await _context.ExamSchedules.FindAsync(id);
            if (schedule == null) return NotFound();

            _context.ExamSchedules.Remove(schedule);
            LogActivity("Removed an exam schedule", "Action Req.");
            await _context.SaveChangesAsync();

            TempData["Success"] = "Exam schedule removed.";
            return RedirectToAction(nameof(Index), new { termId });
        }

        public async Task<IActionResult> DownloadTimetable(int? termId)
        {
            var guard = Guard(); if (guard != null) return guard;

            var schedules = await GetScheduleRows(termId);
            var termName = termId.HasValue
                ? await _context.ExamTerms.Where(t => t.Id == termId.Value).Select(t => t.Name).FirstOrDefaultAsync()
                : "All Exam Terms";

            var builder = new StringBuilder();
            builder.AppendLine("BRAIT INC SMS");
            builder.AppendLine("EXAM TIMETABLE");
            builder.AppendLine(termName ?? "Selected Exam Term");
            builder.AppendLine(new string('-', 72));
            builder.AppendLine($"{"DATE",-12} {"TIME",-17} {"CLASS",-10} {"SUBJECT",-22} {"ROOM",-8} INVIGILATOR");
            builder.AppendLine(new string('-', 72));

            foreach (var item in schedules)
            {
                builder.AppendLine($"{item.ExamDate:dd MMM yyyy,-12} {item.StartTime:hh\\:mm}-{item.EndTime:hh\\:mm,-11} {item.ClassName,-10} {TrimForReceipt(item.SubjectName, 22),-22} {item.RoomNumber,-8} {item.InvigilatorName}");
            }

            if (!schedules.Any())
                builder.AppendLine("No exam schedules found.");

            builder.AppendLine(new string('-', 72));
            builder.AppendLine($"Printed: {DateTime.Now:dd MMM yyyy HH:mm}");

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/plain", "Brait-Inc-Exam-Timetable.txt");
        }

        public async Task<IActionResult> PrintTimetable(int? termId)
        {
            var guard = Guard(); if (guard != null) return guard;

            var termName = termId.HasValue
                ? await _context.ExamTerms.Where(t => t.Id == termId.Value).Select(t => t.Name).FirstOrDefaultAsync()
                : "All Exam Terms";

            var vm = new ExamPrintTimetableViewModel
            {
                TermName = termName ?? "Selected Exam Term",
                Schedules = await GetScheduleRows(termId)
            };

            return View(vm);
        }

        private static string NormalizeTermStatus(string status)
        {
            return status == "Active" || status == "Completed" ? status : "Upcoming";
        }

        private static void ApplySchedule(ExamSchedule schedule, ExamScheduleUpsertViewModel vm)
        {
            schedule.ExamTermId = vm.ExamTermId;
            schedule.ClassId = vm.ClassId;
            schedule.SubjectId = vm.SubjectId;
            schedule.InvigilatorTeacherId = vm.InvigilatorTeacherId;
            schedule.RoomNumber = vm.RoomNumber.Trim();
            schedule.ExamDate = vm.ExamDate.Date;
            schedule.StartTime = vm.StartTime;
            schedule.EndTime = vm.EndTime;
            schedule.Notes = vm.Notes?.Trim() ?? string.Empty;
        }

        private static ExamScheduleRowViewModel ToScheduleRow(ExamSchedule schedule)
        {
            return new ExamScheduleRowViewModel
            {
                Id = schedule.Id,
                ExamTermId = schedule.ExamTermId,
                ClassId = schedule.ClassId,
                SubjectId = schedule.SubjectId,
                InvigilatorTeacherId = schedule.InvigilatorTeacherId,
                TermName = schedule.ExamTerm?.Name ?? "Exam Term",
                ClassName = schedule.Class?.Name ?? "Class",
                SubjectName = schedule.Subject?.Name ?? "Subject",
                InvigilatorName = schedule.InvigilatorTeacher?.FullName ?? "Unassigned",
                RoomNumber = schedule.RoomNumber,
                ExamDate = schedule.ExamDate,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                Notes = schedule.Notes
            };
        }

        private async Task<List<ExamScheduleRowViewModel>> GetScheduleRows(int? termId)
        {
            var query = _context.ExamSchedules
                .Include(s => s.ExamTerm)
                .Include(s => s.Class)
                .Include(s => s.Subject)
                .Include(s => s.InvigilatorTeacher)
                .AsQueryable();

            if (termId.HasValue)
                query = query.Where(s => s.ExamTermId == termId.Value);

            var schedules = await query
                .OrderBy(s => s.ExamDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            return schedules.Select(ToScheduleRow).ToList();
        }

        private static string TrimForReceipt(string value, int length)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return value.Length <= length ? value : value[..Math.Max(0, length - 3)] + "...";
        }
    }
}
