using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class SettingsController : Controller
    {
        private readonly AppDbContext _context;

        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? Guard()
        {
            if (HttpContext.Session.GetString("UserRole") == null)
                return RedirectToAction("Login", "Account");
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var guard = Guard(); if (guard != null) return guard;

            var defaults = new Dictionary<string, string>
            {
                ["SchoolName"] = "Brait Inc SMS",
                ["AcademicYear"] = $"{DateTime.Today.Year}",
                ["CurrentTerm"] = "Current Term",
                ["SupportEmail"] = "support@braitinc.local"
            };

            var stored = await _context.SchoolSettings.ToListAsync();
            foreach (var setting in stored)
                defaults[setting.Key] = setting.Value;

            return View(new SettingsPageViewModel { Settings = defaults });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Dictionary<string, string> settings)
        {
            var guard = Guard(); if (guard != null) return guard;
            if (HttpContext.Session.GetString("UserRole") != "Admin")
            {
                TempData["Error"] = "Only administrators can update school settings.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var pair in settings)
            {
                var item = await _context.SchoolSettings.FirstOrDefaultAsync(s => s.Key == pair.Key);
                if (item == null)
                {
                    item = new SchoolSetting { Key = pair.Key, Group = "General" };
                    _context.SchoolSettings.Add(item);
                }
                item.Value = pair.Value ?? string.Empty;
                item.UpdatedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Settings saved.";
            return RedirectToAction(nameof(Index));
        }
    }
}
