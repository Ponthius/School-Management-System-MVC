using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;
using School_Management_System.Models;
using School_Management_System.Models.ViewModels;

namespace School_Management_System.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
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

        public async Task<IActionResult> Index(string? search)
        {
            var guard = GuardAdmin(); if (guard != null) return guard;

            var normalizedSearch = search?.Trim() ?? string.Empty;
            var paymentQuery = _context.Payments.Include(p => p.Student).AsQueryable();
            if (!string.IsNullOrWhiteSpace(normalizedSearch))
                paymentQuery = paymentQuery.Where(p => p.Student!.FullName.Contains(normalizedSearch) || p.ReceiptNumber.Contains(normalizedSearch));

            var invoices = await _context.FeeInvoices.Include(i => i.Student).OrderByDescending(i => i.CreatedAt).ToListAsync();
            var collected = await _context.Payments.Where(p => p.Status == "Complete" || p.Status == "Partial").SumAsync(p => (decimal?)p.Amount) ?? 0;
            var pending = invoices.Sum(i => i.Balance);
            var due = invoices.Sum(i => i.AmountDue);

            var vm = new PaymentDashboardViewModel
            {
                Search = normalizedSearch,
                TotalCollected = collected,
                TotalPending = pending,
                OverdueCount = invoices.Count(i => i.Balance > 0 && i.DueDate.Date < DateTime.Today),
                CollectionEfficiency = due > 0 ? Math.Round(collected / due * 100, 1) : 0,
                Invoices = invoices,
                Payments = await paymentQuery.OrderByDescending(p => p.Date).Take(50).ToListAsync(),
                Students = await _context.Students.OrderBy(s => s.FullName)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.FullName })
                    .ToListAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Record(PaymentEntryViewModel vm)
        {
            var guard = GuardAdmin(); if (guard != null) return guard;

            if (vm.StudentId <= 0 || vm.Amount <= 0)
            {
                TempData["Error"] = "Select a student and enter a valid payment amount.";
                return RedirectToAction(nameof(Index));
            }

            var student = await _context.Students.FindAsync(vm.StudentId);
            if (student == null) return NotFound();

            var term = string.IsNullOrWhiteSpace(vm.Term) ? "Current Term" : vm.Term.Trim();
            var invoice = await _context.FeeInvoices
                .FirstOrDefaultAsync(i => i.StudentId == vm.StudentId && i.Term == term && i.Balance > 0);

            if (invoice == null)
            {
                var due = vm.FeeAmountDue > 0 ? vm.FeeAmountDue : vm.Amount;
                invoice = new FeeInvoice
                {
                    StudentId = vm.StudentId,
                    Term = term,
                    AmountDue = due,
                    Balance = due,
                    DueDate = DateTime.Today.AddDays(30)
                };
                _context.FeeInvoices.Add(invoice);
            }

            invoice.AmountPaid += vm.Amount;
            invoice.Balance = Math.Max(0, invoice.AmountDue - invoice.AmountPaid);
            invoice.Status = invoice.Balance <= 0 ? "Full Paid" : "Partial";

            var payment = new Payment
            {
                StudentId = vm.StudentId,
                FeeInvoice = invoice,
                Amount = vm.Amount,
                Balance = invoice.Balance,
                Term = term,
                PaymentMethod = vm.PaymentMethod,
                Status = invoice.Balance <= 0 ? "Complete" : "Partial",
                ReceiptNumber = $"BR-{DateTime.Now:yyyyMMddHHmmss}-{vm.StudentId}",
                RecordedByName = HttpContext.Session.GetString("Username") ?? "Admin",
                Notes = vm.Notes?.Trim() ?? string.Empty,
                Date = DateTime.Now
            };

            _context.Payments.Add(payment);
            _context.ActivityLogs.Add(new ActivityLog
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Admin",
                UserRole = "Admin",
                Action = $"Recorded payment for {student.FullName}",
                Status = "Verified",
                RelatedUserId = student.Id
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Payment recorded. Receipt {payment.ReceiptNumber}.";
            return RedirectToAction(nameof(Receipt), new { id = payment.Id });
        }

        public async Task<IActionResult> Receipt(int id)
        {
            var guard = GuardAdmin(); if (guard != null) return guard;

            var payment = await _context.Payments
                .Include(p => p.Student)
                .Include(p => p.FeeInvoice)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (payment == null) return NotFound();

            return View(payment);
        }

        public async Task<IActionResult> DownloadReport()
        {
            var guard = GuardAdmin(); if (guard != null) return guard;

            var payments = await _context.Payments.Include(p => p.Student).OrderByDescending(p => p.Date).ToListAsync();
            var builder = new StringBuilder();
            builder.AppendLine("BRAIT INC SMS - PAYMENT REPORT");
            builder.AppendLine($"Printed: {DateTime.Now:dd MMM yyyy HH:mm}");
            builder.AppendLine(new string('-', 80));
            builder.AppendLine($"{"DATE",-14} {"RECEIPT",-24} {"STUDENT",-24} {"AMOUNT",10} {"BALANCE",10}");
            foreach (var p in payments)
                builder.AppendLine($"{p.Date:dd MMM yyyy,-14} {p.ReceiptNumber,-24} {Trim(p.Student?.FullName ?? "Student", 24),-24} {p.Amount,10:N0} {p.Balance,10:N0}");

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/plain", "Brait-Inc-Payment-Report.txt");
        }

        private static string Trim(string value, int length) =>
            value.Length <= length ? value : value[..Math.Max(0, length - 3)] + "...";
    }
}
