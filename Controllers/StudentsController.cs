using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class StudentsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // ── Guard ──────────────────────────────────────────────
        private IActionResult? Guard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role != "Admin") return RedirectToAction("Admin", "Dashboard");
            return null;
        }

        // ── INDEX ──────────────────────────────────────────────
        public IActionResult Index(string? search, int? classId, int currentPage = 1)
        {
            var guard = Guard(); if (guard != null) return guard;

            const int pageSize = 10;

            var query = _context.Students
                .Include(s => s.Class)
                .Include(s => s.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.FullName.Contains(search));

            if (classId.HasValue && classId > 0)
                query = query.Where(s => s.ClassId == classId);

            int total = query.Count();
            int totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var students = query
                .OrderBy(s => s.FullName)
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.TotalStudents = _context.Students.Count();
            ViewBag.NewEnrolments = _context.Students.Count(s => s.Status == "New");
            ViewBag.AttendanceAlerts = 18;
            ViewBag.UnpaidFees = _context.Payments.Count(p => p.Status == "Pending");

            ViewBag.Classes = new SelectList(_context.Classes.ToList(), "Id", "Name", classId);
            ViewBag.Search = search;
            ViewBag.ClassId = classId;
            ViewBag.Page = currentPage;
            ViewBag.TotalPages = totalPages;
            ViewBag.Total = total;

            return View(students);
        }

        // ── DETAILS ────────────────────────────────────────────
        public IActionResult Details(int id)
        {
            var guard = Guard(); if (guard != null) return guard;

            var student = _context.Students
                .Include(s => s.Class)
                .Include(s => s.User)
                .FirstOrDefault(s => s.Id == id);

            if (student == null) return NotFound();
            return View(student);
        }

        // ── CREATE GET ─────────────────────────────────────────
        public IActionResult Create()
        {
            var guard = Guard(); if (guard != null) return guard;

            ViewBag.Classes = new SelectList(_context.Classes.ToList(), "Id", "Name");
            return View(new StudentCreateViewModel());
        }

        // ── CREATE POST ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentCreateViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            // Check username not taken
            if (_context.Users.Any(u => u.Username == vm.Username))
                ModelState.AddModelError("Username", "Username already exists.");

            if (!ModelState.IsValid)
            {
                ViewBag.Classes = new SelectList(_context.Classes.ToList(), "Id", "Name");
                return View(vm);
            }

            // 1. Create user account
            var user = new User
            {
                Username = vm.Username,
                Password = vm.Password,
                Role = "Student"
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // 2. Handle photo upload
            string photoPath = "/images/default-avatar.png";
            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "students");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Photo.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.Photo.CopyToAsync(stream);
                photoPath = $"/uploads/students/{fileName}";
            }

            // 3. Create student record
            var student = new Student
            {
                UserId = user.Id,
                ClassId = vm.ClassId,
                FullName = vm.FullName,
                Gender = vm.Gender,
                DOB = vm.DOB,
                GuardianName = vm.GuardianName,
                GuardianContact = vm.GuardianContact,
                Status = vm.Status,
                Term = vm.Term,
                PhotoPath = photoPath,
                EnrolledDate = DateTime.Now
            };
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            // 4. Log activity
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action = $"Added {student.FullName} to {_context.Classes.Find(student.ClassId)?.Name}",
                Status = "Verified",
                RelatedUserId = student.Id
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Student {student.FullName} created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── EDIT GET ───────────────────────────────────────────
        public IActionResult Edit(int id)
        {
            var guard = Guard(); if (guard != null) return guard;

            var student = _context.Students
                .Include(s => s.User)
                .FirstOrDefault(s => s.Id == id);

            if (student == null) return NotFound();

            var vm = new StudentEditViewModel
            {
                Id = student.Id,
                FullName = student.FullName,
                Gender = student.Gender,
                DOB = student.DOB,
                ClassId = student.ClassId,
                GuardianName = student.GuardianName,
                GuardianContact = student.GuardianContact,
                Status = student.Status,
                Term = student.Term,
                ExistingPhoto = student.PhotoPath
            };

            ViewBag.Classes = new SelectList(_context.Classes.ToList(), "Id", "Name", student.ClassId);
            return View(vm);
        }

        // ── EDIT POST ──────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentEditViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (!ModelState.IsValid)
            {
                ViewBag.Classes = new SelectList(_context.Classes.ToList(), "Id", "Name", vm.ClassId);
                return View(vm);
            }

            var student = _context.Students.Find(id);
            if (student == null) return NotFound();

            // Handle new photo
            if (vm.Photo != null && vm.Photo.Length > 0)
            {
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "students");
                Directory.CreateDirectory(uploadsDir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(vm.Photo.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.Photo.CopyToAsync(stream);
                student.PhotoPath = $"/uploads/students/{fileName}";
            }

            student.FullName = vm.FullName;
            student.Gender = vm.Gender;
            student.DOB = vm.DOB;
            student.ClassId = vm.ClassId;
            student.GuardianName = vm.GuardianName;
            student.GuardianContact = vm.GuardianContact;
            student.Status = vm.Status;
            student.Term = vm.Term;

            await _context.SaveChangesAsync();

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action = $"Updated record for {student.FullName}",
                Status = "Complete",
                RelatedUserId = student.Id
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{student.FullName} updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // ── DELETE POST ────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var guard = Guard(); if (guard != null) return guard;

            var student = _context.Students
                .Include(s => s.User)
                .FirstOrDefault(s => s.Id == id);

            if (student == null) return NotFound();

            string name = student.FullName;

            // Remove photo file if not default
            if (!string.IsNullOrEmpty(student.PhotoPath)
                && !student.PhotoPath.Contains("default-avatar"))
            {
                var fullPath = Path.Combine(_env.WebRootPath,
                    student.PhotoPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            // Remove associated user account
            var user = student.User;
            _context.Students.Remove(student);
            if (user != null) _context.Users.Remove(user);

            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action = $"Removed student record for {name}",
                Status = "Action Req."
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"{name} has been removed.";
            return RedirectToAction(nameof(Index));
        }
    }
}