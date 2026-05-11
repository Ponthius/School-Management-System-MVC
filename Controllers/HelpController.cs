using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class HelpController : Controller
    {
        private readonly AppDbContext _context;

        public HelpController(AppDbContext context)
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

            var role = HttpContext.Session.GetString("UserRole") ?? "";
            var userId = HttpContext.Session.GetInt32("UserId");
            var tickets = _context.HelpTickets.Include(t => t.User).AsQueryable();
            if (role != "Admin")
                tickets = tickets.Where(t => t.UserId == userId);

            var vm = new HelpPageViewModel
            {
                Tickets = await tickets.OrderByDescending(t => t.CreatedAt).ToListAsync(),
                Notices = await _context.Notices
                    .Where(n => n.Audience == "All" || n.Audience == role)
                    .OrderByDescending(n => n.PublishDate)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(HelpTicketCreateViewModel vm)
        {
            var guard = Guard(); if (guard != null) return guard;

            if (string.IsNullOrWhiteSpace(vm.Subject) || string.IsNullOrWhiteSpace(vm.Message))
            {
                TempData["Error"] = "Subject and message are required.";
                return RedirectToAction(nameof(Index));
            }

            _context.HelpTickets.Add(new HelpTicket
            {
                UserId = HttpContext.Session.GetInt32("UserId"),
                Subject = vm.Subject.Trim(),
                Message = vm.Message.Trim(),
                Priority = vm.Priority
            });
            await _context.SaveChangesAsync();
            TempData["Success"] = "Help request submitted.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseTicket(int id)
        {
            var guard = Guard(); if (guard != null) return guard;
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return Forbid();

            var ticket = await _context.HelpTickets.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.Status = "Closed";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Help ticket closed.";
            return RedirectToAction(nameof(Index));
        }
    }
}
