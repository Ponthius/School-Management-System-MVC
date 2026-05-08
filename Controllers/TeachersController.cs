using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class TeachersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        // All available subjects for the multi-select
        private static readonly List<string> AllSubjects = new()
        {
            "Mathematics", "Physics", "Chemistry", "Biology",
            "English Literature", "English Language", "History",
            "Geography", "Social Studies", "Computer Science",
            "Physical Education", "Fine Art", "Music",
            "Economics", "Commerce", "Agriculture", "CRE", "IRE"
        };

        public TeachersController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private IActionResult? Guard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role != "Admin") return RedirectToAction("Admin", "Dashboard");
            return null;
        }

        // GET: /Teachers
        public IActionResult Index(string? search, int currentPage = 1)
        {
            var guard = Guard(); if (guard != null) return guard;

            const int pageSize = 10;

            var query = _context.Teachers
                .Include(t => t.User)
                .Include(t => t.PrimaryClass)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t =>
                    t.FullName.Contains(search) ||
                    t.Email.Contains(search) ||
                    t.AssignedSubjects.Contains(search));

            int total      = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var teachers = query
                .OrderBy(t => t.FullName)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Stats
            ViewBag.ActiveFaculty   = _context.Teachers.Count(t => t.Status == "Active");
            ViewBag.Departments     = _context.Classes.Select(c => c.Name).Distinct().Count();
            ViewBag.PendingReports  = 0; // placeholder

            ViewBag.Search     = search;
            ViewBag.Page       = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total      = total;

            return View(teachers);
        }

        // GET: /Teachers/Details/5
        public IActionResult Details(int id)
        {
            var guard = Guard(); if (guard != null) return guard;

            var teacher = _context.Teachers
                .Include(t => t.User)
                .Include(t => t.PrimaryClass)
                .FirstOrDefault(t => t.Id == id);

            if (teacher == null) return NotFound();
            return View(teacher);
        }

        // GET: /Teachers/Create
        public IActionResult Create()
        {
            var guard = Guard(); if (guard != null) return guard;

            ViewBag.Classes     = new SelectList(_context.Classes.ToList(), "Id", "Name");
            ViewBag.AllSubjects = AllSubjects;
            return View(new TeacherCreateViewModel());
        }

        // POST: /Teachers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TeacherCreateViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (_context.Users.Any(u => u.Username == vm.Username))
                ModelState.AddModelError("Username", "Username already exists.");

            if (!ModelState.IsValid)
            {
                ViewBag.Classes     = new SelectList(_context.Classes.ToList(), "Id", "Name");
                ViewBag.AllSubjects = AllSubjects;
                return View(vm);
            }

            // 1. Create user account
            var user = new User
            {
                Username = vm.Username,
                Password = vm.Password,
                Role     = "Teacher"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 2. Handle photo
            string photoPath = "/images/default-avatar.png";
            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                var dir      = Path.Combine(_env.WebRootPath, "uploads", "teachers");
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Photo.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await vm.Photo.CopyToAsync(stream);
                photoPath = $"/uploads/teachers/{fileName}";
            }

            // 3. Create teacher
            var teacher = new Teacher
            {
                UserId           = user.Id,
                FullName         = vm.FullName,
                Email            = vm.Email,
                AssignedSubjects = string.Join(",", vm.SelectedSubjects),
                PrimaryClassId   = vm.PrimaryClassId,
                Status           = vm.Status,
                PhotoPath        = photoPath,
                JoinedDate       = DateTime.Now
            };
            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();

            // 4. Activity log
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action   = $"Added teacher {teacher.FullName}",
                Status   = "Verified"
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Teacher {teacher.FullName} added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Teachers/Edit/5
        public IActionResult Edit(int id)
        {
            var guard = Guard(); if (guard != null) return guard;

            var teacher = _context.Teachers.Find(id);
            if (teacher == null) return NotFound();

            var vm = new TeacherEditViewModel
            {
                Id               = teacher.Id,
                FullName         = teacher.FullName,
                Email            = teacher.Email,
                SelectedSubjects = teacher.AssignedSubjects
                                          .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                          .ToList(),
                PrimaryClassId   = teacher.PrimaryClassId,
                Status           = teacher.Status,
                ExistingPhoto    = teacher.PhotoPath
            };

            ViewBag.Classes     = new SelectList(_context.Classes.ToList(), "Id", "Name", teacher.PrimaryClassId);
            ViewBag.AllSubjects = AllSubjects;
            return View(vm);
        }

        // POST: /Teachers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TeacherEditViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                ViewBag.Classes     = new SelectList(_context.Classes.ToList(), "Id", "Name", vm.PrimaryClassId);
                ViewBag.AllSubjects = AllSubjects;
                return View(vm);
            }

            var teacher = _context.Teachers.Find(id);
            if (teacher == null) return NotFound();

            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                var dir      = Path.Combine(_env.WebRootPath, "uploads", "teachers");
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Photo.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await vm.Photo.CopyToAsync(stream);
                teacher.PhotoPath = $"/uploads/teachers/{fileName}";
            }

            teacher.FullName         = vm.FullName;
            teacher.Email            = vm.Email;
            teacher.AssignedSubjects = string.Join(",", vm.SelectedSubjects);
            teacher.PrimaryClassId   = vm.PrimaryClassId;
            teacher.Status           = vm.Status;

            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action   = $"Updated teacher record for {teacher.FullName}",
                Status   = "Complete"
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{teacher.FullName} updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Teachers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var guard = Guard(); if (guard != null) return guard;

            var teacher = _context.Teachers
                .Include(t => t.User)
                .FirstOrDefault(t => t.Id == id);

            if (teacher == null) return NotFound();

            string name = teacher.FullName;

            // Delete photo file
            if (!string.IsNullOrEmpty(teacher.PhotoPath)
                && !teacher.PhotoPath.Contains("default-avatar"))
            {
                var fullPath = Path.Combine(_env.WebRootPath,
                    teacher.PhotoPath.TrimStart('/')
                           .Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            var user = teacher.User;
            _context.Teachers.Remove(teacher);
            if (user != null) _context.Users.Remove(user);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action   = $"Removed teacher record for {name}",
                Status   = "Action Req."
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{name} has been removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}