using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class ClassSubjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ClassSubjectsController(AppDbContext context)
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

        public IActionResult Index(string? search)
        {
            var guard = Guard(); if (guard != null) return guard;

            var normalizedSearch = search?.Trim() ?? string.Empty;

            var classes = _context.Classes
                .Include(c => c.Students)
                .Include(c => c.Teacher)
                .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                .OrderBy(c => c.Name)
                .ToList();

            var subjects = _context.Subjects
                .Include(s => s.ClassSubjects)
                    .ThenInclude(cs => cs.Class)
                .OrderBy(s => s.Name)
                .ToList();

            var teachers = _context.Teachers
                .OrderBy(t => t.FullName)
                .ToList();

            if (!string.IsNullOrWhiteSpace(normalizedSearch))
            {
                classes = classes.Where(c =>
                        c.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                        (c.Teacher?.FullName?.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        c.ClassSubjects.Any(cs => cs.Subject != null &&
                            cs.Subject.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                subjects = subjects.Where(s =>
                        s.Name.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                        s.Code.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase) ||
                        s.Department.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var classCards = classes.Select((cls, index) => new ClassSubjectsClassCardViewModel
            {
                Id = cls.Id,
                Name = cls.Name,
                Capacity = cls.Capacity,
                StudentCount = cls.Students.Count,
                SubjectCount = cls.ClassSubjects.Count,
                ClassTeacherName = cls.Teacher?.FullName ?? "Unassigned",
                TeacherId = cls.TeacherId,
                AccentClass = (index % 3) switch
                {
                    0 => "accent-indigo",
                    1 => "accent-green",
                    _ => "accent-amber"
                },
                Subjects = cls.ClassSubjects
                    .Where(cs => cs.Subject != null)
                    .OrderBy(cs => cs.Subject!.Name)
                    .Select(cs => new ClassSubjectsSubjectChipViewModel
                    {
                        Id = cs.SubjectId,
                        Name = cs.Subject!.Name,
                        Department = cs.Subject.Department
                    })
                    .ToList()
            }).ToList();

            var subjectRows = subjects.Select(subject => new ClassSubjectsSubjectRowViewModel
            {
                Id = subject.Id,
                Code = subject.Code,
                Name = subject.Name,
                Department = subject.Department,
                ActiveClasses = subject.ClassSubjects
                    .Where(cs => cs.Class != null)
                    .OrderBy(cs => cs.Class!.Name)
                    .Select(cs => cs.Class!.Name)
                    .ToList()
            }).ToList();

            var viewModel = new ClassSubjectsDashboardViewModel
            {
                Search = normalizedSearch,
                ClassCards = classCards,
                SubjectRows = subjectRows,
                Stats = new List<ClassSubjectsStatViewModel>
                {
                    new() { Label = "Total Classes", Value = _context.Classes.Count().ToString("D2"), Icon = "bi-building", AccentClass = "accent-indigo" },
                    new() { Label = "Active Subjects", Value = _context.Subjects.Count().ToString("D2"), Icon = "bi-journal-bookmark", AccentClass = "accent-green" },
                    new() { Label = "Unassigned Teachers", Value = _context.Classes.Count(c => c.TeacherId == null).ToString("D2"), Icon = "bi-person-x", AccentClass = "accent-amber" },
                    new() { Label = "Missing Materials", Value = _context.Classes.Count(c => !c.ClassSubjects.Any()).ToString("D2"), Icon = "bi-exclamation-circle", AccentClass = "accent-rose" }
                }
            };

            ViewBag.Teachers = new SelectList(teachers, "Id", "FullName");
            ViewBag.Subjects = new SelectList(_context.Subjects.OrderBy(s => s.Name).ToList(), "Id", "Name");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveClass(ClassUpsertViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Class details are invalid.";
                return RedirectToAction(nameof(Index));
            }

            var duplicate = _context.Classes.Any(c => c.Name == vm.Name && c.Id != vm.Id);
            if (duplicate)
            {
                TempData["Error"] = "A class with that name already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (vm.Id.HasValue)
            {
                var existing = await _context.Classes.FindAsync(vm.Id.Value);
                if (existing == null) return NotFound();

                existing.Name = vm.Name;
                existing.Capacity = vm.Capacity;
                LogActivity($"Updated class {existing.Name}");
                TempData["Success"] = $"Updated {existing.Name}.";
            }
            else
            {
                _context.Classes.Add(new Class
                {
                    Name = vm.Name,
                    Capacity = vm.Capacity
                });
                LogActivity($"Created class {vm.Name}", "Verified");
                TempData["Success"] = $"Created class {vm.Name}.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSubject(SubjectUpsertViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Subject details are invalid.";
                return RedirectToAction(nameof(Index));
            }

            var normalizedCode = vm.Code.Trim().ToUpperInvariant();
            var duplicateCode = _context.Subjects.Any(s => s.Code == normalizedCode && s.Id != vm.Id);
            if (duplicateCode)
            {
                TempData["Error"] = "That subject code already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (vm.Id.HasValue)
            {
                var existing = await _context.Subjects.FindAsync(vm.Id.Value);
                if (existing == null) return NotFound();

                existing.Code = normalizedCode;
                existing.Name = vm.Name;
                existing.Department = vm.Department;
                LogActivity($"Updated subject {existing.Name}");
                TempData["Success"] = $"Updated subject {existing.Name}.";
            }
            else
            {
                _context.Subjects.Add(new Subject
                {
                    Code = normalizedCode,
                    Name = vm.Name,
                    Department = vm.Department
                });
                LogActivity($"Created subject {vm.Name}", "Verified");
                TempData["Success"] = $"Created subject {vm.Name}.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTeacher(ClassTeacherAssignViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            var cls = await _context.Classes.FindAsync(vm.ClassId);
            if (cls == null) return NotFound();

            cls.TeacherId = vm.TeacherId;
            var teacherName = vm.TeacherId.HasValue
                ? await _context.Teachers
                    .Where(t => t.Id == vm.TeacherId.Value)
                    .Select(t => t.FullName)
                    .FirstOrDefaultAsync()
                : null;

            LogActivity(vm.TeacherId.HasValue
                ? $"Assigned {teacherName ?? "a teacher"} to {cls.Name}"
                : $"Cleared class teacher for {cls.Name}");

            await _context.SaveChangesAsync();

            TempData["Success"] = vm.TeacherId.HasValue
                ? $"Assigned a class teacher to {cls.Name}."
                : $"Cleared class teacher for {cls.Name}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSubjectToClass(ClassSubjectAssignViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            var exists = await _context.ClassSubjects.FindAsync(vm.ClassId, vm.SubjectId);
            if (exists != null)
            {
                TempData["Error"] = "That subject is already linked to the class.";
                return RedirectToAction(nameof(Index));
            }

            _context.ClassSubjects.Add(new ClassSubject
            {
                ClassId = vm.ClassId,
                SubjectId = vm.SubjectId
            });

            var className = await _context.Classes
                .Where(c => c.Id == vm.ClassId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
            var subjectName = await _context.Subjects
                .Where(s => s.Id == vm.SubjectId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
            LogActivity($"Linked {subjectName ?? "subject"} to {className ?? "class"}", "Verified");

            await _context.SaveChangesAsync();

            TempData["Success"] = "Linked subject to class.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveSubjectFromClass(ClassSubjectRemoveViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            var link = await _context.ClassSubjects.FindAsync(vm.ClassId, vm.SubjectId);
            if (link == null)
            {
                TempData["Error"] = "That class-subject link no longer exists.";
                return RedirectToAction(nameof(Index));
            }

            _context.ClassSubjects.Remove(link);
            var className = await _context.Classes
                .Where(c => c.Id == vm.ClassId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();
            var subjectName = await _context.Subjects
                .Where(s => s.Id == vm.SubjectId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync();
            LogActivity($"Removed {subjectName ?? "subject"} from {className ?? "class"}", "Action Req.");
            await _context.SaveChangesAsync();

            TempData["Success"] = "Removed subject from class.";
            return RedirectToAction(nameof(Index));
        }
    }
}
