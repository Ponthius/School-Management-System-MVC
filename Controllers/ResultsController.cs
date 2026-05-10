using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class ResultsController : Controller
    {
        private readonly AppDbContext _context;

        public ResultsController(AppDbContext context)
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

        public async Task<IActionResult> Index(int? examTermId, int? classId, int? subjectId, string? search)
        {
            var guard = Guard(); if (guard != null) return guard;

            var terms = await _context.ExamTerms.OrderByDescending(t => t.StartDate).ToListAsync();
            var classes = await _context.Classes.OrderBy(c => c.Name).ToListAsync();

            if (!examTermId.HasValue && terms.Any())
                examTermId = terms.First().Id;

            if (!classId.HasValue && classes.Any())
                classId = classes.First().Id;

            var subjectOptions = classId.HasValue
                ? await GetSubjectsForClass(classId.Value)
                : await _context.Subjects.OrderBy(s => s.Name).ToListAsync();

            if (!subjectId.HasValue && subjectOptions.Any())
                subjectId = subjectOptions.First().Id;

            var normalizedSearch = search?.Trim() ?? string.Empty;

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
            var subjectIds = subjectOptions.Select(s => s.Id).ToList();

            var results = examTermId.HasValue && studentIds.Any()
                ? await _context.ExamResults
                    .Where(r => r.ExamTermId == examTermId.Value && studentIds.Contains(r.StudentId))
                    .ToListAsync()
                : new List<ExamResult>();

            var selectedSubjectResults = subjectId.HasValue
                ? results.Where(r => r.SubjectId == subjectId.Value).ToDictionary(r => r.StudentId)
                : new Dictionary<int, ExamResult>();

            var rows = students.Select((student, index) =>
            {
                selectedSubjectResults.TryGetValue(student.Id, out var selectedResult);
                var average = CalculateStudentAverage(student.Id, subjectIds, results);

                return new ResultStudentRowViewModel
                {
                    StudentId = student.Id,
                    StudentCode = $"#{student.Id:D3}",
                    StudentName = student.FullName,
                    PhotoPath = student.PhotoPath,
                    Score = selectedResult?.Score,
                    TeacherComments = selectedResult?.TeacherComments ?? string.Empty,
                    Status = GetScoreStatus(selectedResult?.Score),
                    Average = average,
                    Grade = subjectIds.Any() ? GetGrade(average) : "N/A"
                };
            }).ToList();

            var enteredScores = rows.Where(r => r.Score.HasValue).Select(r => r.Score!.Value).ToList();
            var totalPossibleScores = students.Count * subjectIds.Count;
            var enteredAllSubjectScores = results.Count(r => subjectIds.Contains(r.SubjectId) && r.Score.HasValue);

            var vm = new ResultsEntryViewModel
            {
                ExamTermId = examTermId,
                ClassId = classId,
                SubjectId = subjectId,
                Search = normalizedSearch,
                ExamTermOptions = terms.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = examTermId == t.Id
                }).ToList(),
                ClassOptions = classes.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name,
                    Selected = classId == c.Id
                }).ToList(),
                SubjectOptions = subjectOptions.Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = $"{s.Code} - {s.Name}",
                    Selected = subjectId == s.Id
                }).ToList(),
                Students = rows,
                TotalStudents = students.Count,
                CurrentAverage = rows.Any() ? Math.Round(rows.Average(r => r.Average), 1) : 0,
                MissingScores = Math.Max(0, totalPossibleScores - enteredAllSubjectScores),
                SelectedTermName = terms.FirstOrDefault(t => t.Id == examTermId)?.Name ?? "No exam term",
                SelectedClassName = classes.FirstOrDefault(c => c.Id == classId)?.Name ?? "No class",
                SelectedSubjectName = subjectOptions.FirstOrDefault(s => s.Id == subjectId)?.Name ?? "No subject"
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveScores(ResultsSaveViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            var validationRedirect = await UpsertScores(vm);
            if (validationRedirect != null) return validationRedirect;

            LogActivity("Saved exam results draft", "Verified");
            await _context.SaveChangesAsync();

            TempData["Success"] = "Results draft saved. Missing subject scores still count as zero in averages.";
            return RedirectToAction(nameof(Index), new { examTermId = vm.ExamTermId, classId = vm.ClassId, subjectId = vm.SubjectId, search = vm.Search });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Publish(ResultsSaveViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            var validationRedirect = await UpsertScores(vm);
            if (validationRedirect != null) return validationRedirect;

            var results = await _context.ExamResults
                .Where(r => r.ExamTermId == vm.ExamTermId)
                .ToListAsync();

            foreach (var result in results)
                result.IsPublished = true;

            LogActivity("Published exam results", "Verified");
            await _context.SaveChangesAsync();

            TempData["Success"] = "Results published for the selected exam term.";
            return RedirectToAction(nameof(Index), new { examTermId = vm.ExamTermId, classId = vm.ClassId, subjectId = vm.SubjectId, search = vm.Search });
        }

        private async Task<IActionResult?> UpsertScores(ResultsSaveViewModel vm)
        {
            if (vm.ExamTermId <= 0 || vm.ClassId <= 0 || vm.SubjectId <= 0)
            {
                TempData["Error"] = "Select an exam term, class, and subject before saving marks.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var item in vm.Scores)
            {
                if (item.Score.HasValue && (item.Score.Value < 0 || item.Score.Value > 100))
                {
                    TempData["Error"] = "Scores must be between 0 and 100.";
                    return RedirectToAction(nameof(Index), new { examTermId = vm.ExamTermId, classId = vm.ClassId, subjectId = vm.SubjectId, search = vm.Search });
                }

                var result = await _context.ExamResults.FirstOrDefaultAsync(r =>
                    r.ExamTermId == vm.ExamTermId &&
                    r.StudentId == item.StudentId &&
                    r.SubjectId == vm.SubjectId);

                if (result == null)
                {
                    result = new ExamResult
                    {
                        ExamTermId = vm.ExamTermId,
                        StudentId = item.StudentId,
                        SubjectId = vm.SubjectId
                    };
                    _context.ExamResults.Add(result);
                }

                result.Score = item.Score;
                result.TeacherComments = item.TeacherComments?.Trim() ?? string.Empty;
                result.RecordedByName = HttpContext.Session.GetString("Username") ?? "Admin";
                result.UpdatedAt = DateTime.Now;
            }

            return null;
        }

        private async Task<List<Subject>> GetSubjectsForClass(int classId)
        {
            var linkedSubjects = await _context.ClassSubjects
                .Where(cs => cs.ClassId == classId)
                .Include(cs => cs.Subject)
                .Select(cs => cs.Subject!)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return linkedSubjects.Any()
                ? linkedSubjects
                : await _context.Subjects.OrderBy(s => s.Name).ToListAsync();
        }

        private static decimal CalculateStudentAverage(int studentId, List<int> subjectIds, List<ExamResult> results)
        {
            if (!subjectIds.Any()) return 0;

            decimal total = 0;
            foreach (var subjectId in subjectIds)
            {
                var score = results.FirstOrDefault(r => r.StudentId == studentId && r.SubjectId == subjectId)?.Score ?? 0;
                total += score;
            }

            return Math.Round(total / subjectIds.Count, 1);
        }

        private static string GetScoreStatus(decimal? score)
        {
            if (!score.HasValue) return "Missing";
            if (score.Value >= 50) return "Passed";
            if (score.Value >= 40) return "Marginal";
            return "Failed";
        }

        private static string GetGrade(decimal average)
        {
            if (average >= 80) return "A";
            if (average >= 70) return "B";
            if (average >= 60) return "C";
            if (average >= 50) return "D";
            return "F";
        }
    }
}
