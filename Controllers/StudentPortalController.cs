using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class StudentPortalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentPortalController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private IActionResult? GuardStudent()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role != "Student") return RedirectToAction("Login", "Account");
            return null;
        }

        private async Task<Student?> CurrentStudent()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return null;
            return await _context.Students.Include(s => s.Class).Include(s => s.User).FirstOrDefaultAsync(s => s.UserId == userId.Value);
        }

        public async Task<IActionResult> Profile()
        {
            var guard = GuardStudent(); if (guard != null) return guard;
            var student = await CurrentStudent();
            if (student == null) return NotFound();
            return View(student);
        }

        public async Task<IActionResult> Results()
        {
            var guard = GuardStudent(); if (guard != null) return guard;
            var student = await CurrentStudent();
            if (student == null) return NotFound();

            var subjects = await SubjectsForStudent(student);
            var results = await _context.ExamResults
                .Include(r => r.Subject)
                .Include(r => r.ExamTerm)
                .Where(r => r.StudentId == student.Id && r.IsPublished)
                .ToListAsync();

            var rows = subjects.Select(subject =>
            {
                var score = results
                    .Where(r => r.SubjectId == subject.Id)
                    .OrderByDescending(r => r.UpdatedAt)
                    .FirstOrDefault()?.Score ?? 0;
                return new SubjectResultSummaryViewModel
                {
                    SubjectName = subject.Name,
                    SubjectCode = subject.Code,
                    Score = score,
                    Grade = Grade(score),
                    Status = score >= 80 ? "Excellent" : score >= 50 ? "Passed" : score > 0 ? "Needs Review" : "Missing"
                };
            }).ToList();

            var avg = rows.Any() ? Math.Round(rows.Average(r => r.Score), 1) : 0;
            return View(new StudentResultsViewModel
            {
                Student = student,
                Subjects = rows,
                Average = avg,
                Grade = rows.Any() ? Grade(avg) : "N/A"
            });
        }

        public async Task<IActionResult> Fees()
        {
            var guard = GuardStudent(); if (guard != null) return guard;
            var student = await CurrentStudent();
            if (student == null) return NotFound();

            var invoices = await _context.FeeInvoices.Where(i => i.StudentId == student.Id).OrderByDescending(i => i.CreatedAt).ToListAsync();
            var payments = await _context.Payments.Where(p => p.StudentId == student.Id).OrderByDescending(p => p.Date).ToListAsync();
            return View(new StudentFeesViewModel
            {
                Student = student,
                Invoices = invoices,
                Payments = payments,
                Balance = invoices.Sum(i => i.Balance)
            });
        }

        public async Task<IActionResult> Schedule()
        {
            var guard = GuardStudent(); if (guard != null) return guard;
            var student = await CurrentStudent();
            if (student == null) return NotFound();

            var schedules = await _context.ExamSchedules
                .Include(s => s.ExamTerm)
                .Include(s => s.Subject)
                .Include(s => s.InvigilatorTeacher)
                .Where(s => s.ClassId == student.ClassId)
                .OrderBy(s => s.ExamDate)
                .ThenBy(s => s.StartTime)
                .ToListAsync();
            var events = await _context.AcademicCalendarEvents
                .Where(e => e.Audience == "All" || e.Audience == "Student")
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return View(new StudentScheduleViewModel { Student = student, Schedules = schedules, Events = events });
        }

        public async Task<IActionResult> Materials()
        {
            var guard = GuardStudent(); if (guard != null) return guard;
            var student = await CurrentStudent();
            if (student == null) return NotFound();

            var materials = await _context.LearningMaterials
                .Include(m => m.Teacher)
                .Include(m => m.Class)
                .Include(m => m.Subject)
                .Where(m => m.ClassId == null || m.ClassId == student.ClassId)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();
            var assignments = await _context.Assignments
                .Include(a => a.Teacher)
                .Include(a => a.Subject)
                .Include(a => a.Submissions)
                .Where(a => a.ClassId == student.ClassId)
                .OrderBy(a => a.DueDate)
                .ToListAsync();
            var submissions = await _context.AssignmentSubmissions
                .Where(s => s.StudentId == student.Id)
                .ToListAsync();

            ViewBag.Assignments = new StudentAssignmentsViewModel { Student = student, Assignments = assignments, Submissions = submissions };
            return View(new MaterialsPageViewModel { Materials = materials });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(AssignmentSubmitViewModel vm)
        {
            var guard = GuardStudent(); if (guard != null) return guard;
            var student = await CurrentStudent();
            if (student == null) return NotFound();

            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.Id == vm.AssignmentId && a.ClassId == student.ClassId);
            if (assignment == null) return NotFound();

            var submission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == vm.AssignmentId && s.StudentId == student.Id);
            if (submission == null)
            {
                submission = new AssignmentSubmission { AssignmentId = vm.AssignmentId, StudentId = student.Id };
                _context.AssignmentSubmissions.Add(submission);
            }

            if (vm.File != null && vm.File.Length > 0)
                submission.FilePath = await SaveUpload(vm.File, "submissions");
            submission.Notes = vm.Notes?.Trim() ?? string.Empty;
            submission.Status = "Submitted";
            submission.SubmittedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Assignment submitted.";
            return RedirectToAction(nameof(Materials));
        }

        public async Task<IActionResult> Notices()
        {
            var guard = GuardStudent(); if (guard != null) return guard;
            var notices = await _context.Notices
                .Where(n => n.Audience == "All" || n.Audience == "Student")
                .OrderByDescending(n => n.PublishDate)
                .ToListAsync();
            return View(notices);
        }

        private async Task<List<Subject>> SubjectsForStudent(Student student)
        {
            var subjects = await _context.ClassSubjects
                .Where(cs => cs.ClassId == student.ClassId)
                .Include(cs => cs.Subject)
                .Select(cs => cs.Subject!)
                .OrderBy(s => s.Name)
                .ToListAsync();
            return subjects.Any() ? subjects : await _context.Subjects.OrderBy(s => s.Name).ToListAsync();
        }

        private async Task<string> SaveUpload(IFormFile file, string area)
        {
            var dir = Path.Combine(_env.WebRootPath, "uploads", area);
            Directory.CreateDirectory(dir);
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(dir, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/{area}/{fileName}";
        }

        private static string Grade(decimal score)
        {
            if (score >= 80) return "A";
            if (score >= 70) return "B";
            if (score >= 60) return "C";
            if (score >= 50) return "D";
            return "F";
        }
    }
}
