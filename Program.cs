using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using School_Management_System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add MVC
builder.Services.AddControllersWithViews();

// Keep session cookie encryption keys stable for this local app.
var dataProtectionKeysPath = Path.Combine(
    builder.Environment.ContentRootPath,
    "data-protection-keys");
Directory.CreateDirectory(dataProtectionKeysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath))
    .SetApplicationName("SchoolManagementSystem");

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add DbContext with SQL Server LocalDB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable Session BEFORE Authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "students",
    pattern: "Students/{action=Index}/{id?}",
    defaults: new { controller = "Students" });

app.MapControllerRoute(
    name: "teachers",
    pattern: "Teachers/{action=Index}/{id?}",
    defaults: new { controller = "Teachers" });

app.MapControllerRoute(
    name: "attendance",
    pattern: "Attendance/{action=Index}/{id?}",
    defaults: new { controller = "Attendance" });

app.MapControllerRoute(
    name: "exams",
    pattern: "Exams/{action=Index}/{id?}",
    defaults: new { controller = "Exams" });

app.MapControllerRoute(
    name: "results",
    pattern: "Results/{action=Index}/{id?}",
    defaults: new { controller = "Results" });

app.MapControllerRoute(
    name: "mvc-index-default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
