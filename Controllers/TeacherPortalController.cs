using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class TeacherPortalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TeacherPortalController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private IActionResult? GuardTeacher()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role != "Teacher") return RedirectToAction("Login", "Account");
            return null;
        }

        private async Task<Teacher?> CurrentTeacher()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return null;
            return await _context.Teachers
                .Include(t => t.PrimaryClass)
                .FirstOrDefaultAsync(t => t.UserId == userId.Value);
        }

        private async Task<List<Class>> TeacherClasses(Teacher teacher)
        {
            var assignedSubjectNames = teacher.AssignedSubjects
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var classes = await _context.Classes
                .Include(c => c.Students)
                .Include(c => c.ClassSubjects)
                    .ThenInclude(cs => cs.Subject)
                .Where(c => c.TeacherId == teacher.Id || c.Id == teacher.PrimaryClassId)
                .OrderBy(c => c.Name)
                .ToListAsync();

            if (assignedSubjectNames.Any())
            {
                var subjectClasses = await _context.Classes
                    .Include(c => c.Students)
                    .Include(c => c.ClassSubjects)
                        .ThenInclude(cs => cs.Subject)
                    .Where(c => c.ClassSubjects.Any(cs => cs.Subject != null && assignedSubjectNames.Contains(cs.Subject.Name)))
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                classes = classes.Concat(subjectClasses)
                    .GroupBy(c => c.Id)
                    .Select(g => g.First())
                    .OrderBy(c => c.Name)
                    .ToList();
            }

            return classes;
        }

        public async Task<IActionResult> Classes()
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            return View(new TeacherClassesViewModel
            {
                Teacher = teacher,
                Classes = await TeacherClasses(teacher)
            });
        }

        public async Task<IActionResult> Attendance(int? classId, DateTime? date)
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            var classes = await TeacherClasses(teacher);
            if (!classId.HasValue && classes.Any()) classId = classes.First().Id;
            var selectedDate = (date ?? DateTime.Today).Date;

            var students = classId.HasValue
                ? await _context.Students.Include(s => s.Class).Where(s => s.ClassId == classId.Value).OrderBy(s => s.FullName).ToListAsync()
                : new List<Student>();
            var studentIds = students.Select(s => s.Id).ToList();
            var records = await _context.AttendanceRecords
                .Where(a => a.AttendanceDate == selectedDate && studentIds.Contains(a.StudentId))
                .ToListAsync();

            var rows = students.Select(s =>
            {
                var record = records.FirstOrDefault(r => r.StudentId == s.Id);
                return new AttendanceStudentRowViewModel
                {
                    StudentId = s.Id,
                    ClassId = s.ClassId,
                    StudentName = s.FullName,
                    ClassName = s.Class?.Name ?? "",
                    PhotoPath = s.PhotoPath,
                    Status = record?.Status ?? "Present",
                    Notes = record?.Notes ?? string.Empty
                };
            }).ToList();

            var vm = new TeacherAttendanceViewModel
            {
                ClassId = classId,
                Date = selectedDate,
                Classes = classes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = classId == c.Id }).ToList(),
                Students = rows,
                PresentCount = rows.Count(r => r.Status == "Present"),
                AbsentCount = rows.Count(r => r.Status == "Absent"),
                ExcusedCount = rows.Count(r => r.Status == "Excused")
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(TeacherAttendanceSaveViewModel vm)
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            var allowedClassIds = (await TeacherClasses(teacher)).Select(c => c.Id).ToHashSet();
            if (!allowedClassIds.Contains(vm.ClassId)) return Forbid();

            var date = vm.AttendanceDate.Date;
            foreach (var row in vm.Rows)
            {
                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == row.StudentId && s.ClassId == vm.ClassId);
                if (student == null) continue;

                var record = await _context.AttendanceRecords.FirstOrDefaultAsync(a => a.StudentId == row.StudentId && a.AttendanceDate == date);
                if (record == null)
                {
                    record = new AttendanceRecord { StudentId = row.StudentId, ClassId = vm.ClassId, AttendanceDate = date, MarkedAt = DateTime.Now };
                    _context.AttendanceRecords.Add(record);
                }

                record.Status = row.Status;
                record.Notes = row.Notes?.Trim() ?? string.Empty;
                record.Source = "Teacher";
                record.MarkedByTeacherId = teacher.Id;
                record.MarkedByName = teacher.FullName;
                record.UpdatedAt = DateTime.Now;
            }

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = teacher.FullName,
                UserRole = "Teacher",
                Action = $"Submitted attendance for {date:dd MMM yyyy}",
                Status = "Verified"
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Attendance submitted.";
            return RedirectToAction(nameof(Attendance), new { classId = vm.ClassId, date = date.ToString("yyyy-MM-dd") });
        }

        public async Task<IActionResult> Marks(int? examTermId, int? classId, int? subjectId)
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            var classes = await TeacherClasses(teacher);
            if (!classId.HasValue && classes.Any()) classId = classes.First().Id;

            var terms = await _context.ExamTerms.OrderByDescending(t => t.StartDate).ToListAsync();
            if (!examTermId.HasValue && terms.Any()) examTermId = terms.First().Id;

            var subjects = await TeacherSubjects(teacher, classId);
            if (!subjectId.HasValue && subjects.Any()) subjectId = subjects.First().Id;

            var students = classId.HasValue
                ? await _context.Students.Where(s => s.ClassId == classId.Value).OrderBy(s => s.FullName).ToListAsync()
                : new List<Student>();
            var studentIds = students.Select(s => s.Id).ToList();
            var results = examTermId.HasValue && subjectId.HasValue
                ? await _context.ExamResults.Where(r => r.ExamTermId == examTermId.Value && r.SubjectId == subjectId.Value && studentIds.Contains(r.StudentId)).ToListAsync()
                : new List<ExamResult>();

            var rows = students.Select(s =>
            {
                var result = results.FirstOrDefault(r => r.StudentId == s.Id);
                return new ResultStudentRowViewModel
                {
                    StudentId = s.Id,
                    StudentCode = $"#{s.Id:D3}",
                    StudentName = s.FullName,
                    PhotoPath = s.PhotoPath,
                    Score = result?.Score,
                    TeacherComments = result?.TeacherComments ?? string.Empty,
                    Status = result?.Score >= 50 ? "Passed" : result?.Score == null ? "Missing" : "Failed"
                };
            }).ToList();

            return View(new TeacherMarksViewModel
            {
                ExamTermId = examTermId,
                ClassId = classId,
                SubjectId = subjectId,
                Terms = terms.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = t.Name, Selected = examTermId == t.Id }).ToList(),
                Classes = classes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name, Selected = classId == c.Id }).ToList(),
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name, Selected = subjectId == s.Id }).ToList(),
                Students = rows
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveMarks(ResultsSaveViewModel vm)
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            var allowedClassIds = (await TeacherClasses(teacher)).Select(c => c.Id).ToHashSet();
            if (!allowedClassIds.Contains(vm.ClassId)) return Forbid();

            foreach (var row in vm.Scores)
            {
                if (row.Score.HasValue && (row.Score.Value < 0 || row.Score.Value > 100))
                {
                    TempData["Error"] = "Scores must be between 0 and 100.";
                    return RedirectToAction(nameof(Marks), new { examTermId = vm.ExamTermId, classId = vm.ClassId, subjectId = vm.SubjectId });
                }

                var result = await _context.ExamResults.FirstOrDefaultAsync(r => r.ExamTermId == vm.ExamTermId && r.StudentId == row.StudentId && r.SubjectId == vm.SubjectId);
                if (result == null)
                {
                    result = new ExamResult { ExamTermId = vm.ExamTermId, StudentId = row.StudentId, SubjectId = vm.SubjectId };
                    _context.ExamResults.Add(result);
                }
                result.Score = row.Score;
                result.TeacherComments = row.TeacherComments?.Trim() ?? string.Empty;
                result.RecordedByName = teacher.FullName;
                result.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Marks saved.";
            return RedirectToAction(nameof(Marks), new { examTermId = vm.ExamTermId, classId = vm.ClassId, subjectId = vm.SubjectId });
        }

        public async Task<IActionResult> Materials()
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            var classes = await TeacherClasses(teacher);
            var subjects = await TeacherSubjects(teacher, null);
            var materials = await _context.LearningMaterials
                .Include(m => m.Class)
                .Include(m => m.Subject)
                .Where(m => m.TeacherId == teacher.Id)
                .OrderByDescending(m => m.UploadedAt)
                .ToListAsync();

            return View(new MaterialsPageViewModel
            {
                Materials = materials,
                Classes = classes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMaterial(LearningMaterialCreateViewModel vm)
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            if (string.IsNullOrWhiteSpace(vm.Title))
            {
                TempData["Error"] = "Material title is required.";
                return RedirectToAction(nameof(Materials));
            }

            string filePath = string.Empty;
            if (vm.File != null && vm.File.Length > 0)
                filePath = await SaveUpload(vm.File, "materials");

            _context.LearningMaterials.Add(new LearningMaterial
            {
                TeacherId = teacher.Id,
                ClassId = vm.ClassId,
                SubjectId = vm.SubjectId,
                Title = vm.Title.Trim(),
                Description = vm.Description?.Trim() ?? string.Empty,
                FilePath = filePath,
                MaterialType = vm.MaterialType
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Learning material uploaded.";
            return RedirectToAction(nameof(Materials));
        }

        public async Task<IActionResult> Assignments()
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            var classes = await TeacherClasses(teacher);
            var subjects = await TeacherSubjects(teacher, null);
            var assignments = await _context.Assignments
                .Include(a => a.Class)
                .Include(a => a.Subject)
                .Include(a => a.Submissions)
                .Where(a => a.TeacherId == teacher.Id)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(new AssignmentsPageViewModel
            {
                Assignments = assignments,
                Classes = classes.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList(),
                Subjects = subjects.Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name }).ToList()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(AssignmentCreateViewModel vm)
        {
            var guard = GuardTeacher(); if (guard != null) return guard;
            var teacher = await CurrentTeacher();
            if (teacher == null) return NotFound();

            var allowedClassIds = (await TeacherClasses(teacher)).Select(c => c.Id).ToHashSet();
            if (!allowedClassIds.Contains(vm.ClassId)) return Forbid();
            if (string.IsNullOrWhiteSpace(vm.Title))
            {
                TempData["Error"] = "Assignment title is required.";
                return RedirectToAction(nameof(Assignments));
            }

            _context.Assignments.Add(new Assignment
            {
                TeacherId = teacher.Id,
                ClassId = vm.ClassId,
                SubjectId = vm.SubjectId,
                Title = vm.Title.Trim(),
                Description = vm.Description?.Trim() ?? string.Empty,
                DueDate = vm.DueDate.Date,
                MaxScore = vm.MaxScore
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Assignment created.";
            return RedirectToAction(nameof(Assignments));
        }

        private async Task<List<Subject>> TeacherSubjects(Teacher teacher, int? classId)
        {
            var assignedNames = teacher.AssignedSubjects
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var query = _context.Subjects.AsQueryable();
            if (assignedNames.Any())
                query = query.Where(s => assignedNames.Contains(s.Name));

            var subjects = await query.OrderBy(s => s.Name).ToListAsync();
            if (!subjects.Any() && classId.HasValue)
            {
                subjects = await _context.ClassSubjects
                    .Where(cs => cs.ClassId == classId.Value)
                    .Include(cs => cs.Subject)
                    .Select(cs => cs.Subject!)
                    .OrderBy(s => s.Name)
                    .ToListAsync();
            }

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
    }
}
