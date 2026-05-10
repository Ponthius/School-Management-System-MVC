using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class CalendarController : Controller
    {
        private readonly AppDbContext _context;

        public CalendarController(AppDbContext context)
        {
            _context = context;
        }

        private IActionResult? GuardAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role == null) return RedirectToAction("Login", "Account");
            if (role != "Admin") return RedirectToAction("Admin", "Dashboard");
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var guard = GuardAdmin(); if (guard != null) return guard;

            ViewBag.Notices = await _context.Notices.OrderByDescending(n => n.PublishDate).ToListAsync();
            var events = await _context.AcademicCalendarEvents.OrderBy(e => e.EventDate).ThenBy(e => e.StartTime).ToListAsync();
            return View(events);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEvent(CalendarEventViewModel vm)
        {
            var guard = GuardAdmin(); if (guard != null) return guard;
            if (string.IsNullOrWhiteSpace(vm.Title))
            {
                TempData["Error"] = "Event title is required.";
                return RedirectToAction(nameof(Index));
            }

            var item = vm.Id.HasValue
                ? await _context.AcademicCalendarEvents.FindAsync(vm.Id.Value)
                : new AcademicCalendarEvent();
            if (item == null) return NotFound();

            item.Title = vm.Title.Trim();
            item.Description = vm.Description?.Trim() ?? string.Empty;
            item.EventDate = vm.EventDate.Date;
            item.StartTime = vm.StartTime;
            item.EndTime = vm.EndTime;
            item.Location = vm.Location?.Trim() ?? string.Empty;
            item.Audience = vm.Audience;

            if (!vm.Id.HasValue) _context.AcademicCalendarEvents.Add(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Calendar event saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveNotice(NoticeViewModel vm)
        {
            var guard = GuardAdmin(); if (guard != null) return guard;
            if (string.IsNullOrWhiteSpace(vm.Title) || string.IsNullOrWhiteSpace(vm.Body))
            {
                TempData["Error"] = "Notice title and body are required.";
                return RedirectToAction(nameof(Index));
            }

            var item = vm.Id.HasValue ? await _context.Notices.FindAsync(vm.Id.Value) : new Notice();
            if (item == null) return NotFound();

            item.Title = vm.Title.Trim();
            item.Body = vm.Body.Trim();
            item.Audience = vm.Audience;
            item.Priority = vm.Priority;
            item.PublishDate = vm.PublishDate.Date;

            if (!vm.Id.HasValue) _context.Notices.Add(item);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Notice saved.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var guard = GuardAdmin(); if (guard != null) return guard;
            var item = await _context.AcademicCalendarEvents.FindAsync(id);
            if (item == null) return NotFound();
            _context.AcademicCalendarEvents.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
