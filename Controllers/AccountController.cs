using Microsoft.AspNetCore.Mvc;
using School_Management_System.Data;
using School_Management_System.Models;

namespace School_Management_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            // If already logged in, redirect to dashboard
            if (HttpContext.Session.GetString("UserRole") != null)
                return RedirectToAction("Index", "Dashboard");

            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == model.Username &&
                u.Password == model.Password);

            if (user == null)
            {
                model.ErrorMessage = "Invalid username or password.";
                return View(model);
            }

            // Store session
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            // Redirect based on role
            return user.Role switch
            {
                "Admin" => RedirectToAction("Admin", "Dashboard"),
                "Teacher" => RedirectToAction("Teacher", "Dashboard"),
                "Student" => RedirectToAction("Student", "Dashboard"),
                _ => RedirectToAction("Login")
            };
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}