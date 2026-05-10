using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace School_Management_System.Models.ViewModels
{
    public class PaymentDashboardViewModel
    {
        public string Search { get; set; } = string.Empty;
        public decimal TotalCollected { get; set; }
        public decimal TotalPending { get; set; }
        public int OverdueCount { get; set; }
        public decimal CollectionEfficiency { get; set; }
        public List<Payment> Payments { get; set; } = new();
        public List<FeeInvoice> Invoices { get; set; } = new();
        public List<SelectListItem> Students { get; set; } = new();
    }

    public class PaymentEntryViewModel
    {
        public int StudentId { get; set; }
        public decimal Amount { get; set; }
        public decimal FeeAmountDue { get; set; }
        public string Term { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = "Cash";
        public string Notes { get; set; } = string.Empty;
    }

    public class CalendarEventViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.Today;
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Audience { get; set; } = "All";
    }

    public class NoticeViewModel
    {
        public int? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Audience { get; set; } = "All";
        public string Priority { get; set; } = "Normal";
        public DateTime PublishDate { get; set; } = DateTime.Today;
    }

    public class SettingsPageViewModel
    {
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    public class HelpPageViewModel
    {
        public List<HelpTicket> Tickets { get; set; } = new();
        public List<Notice> Notices { get; set; } = new();
    }

    public class HelpTicketCreateViewModel
    {
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal";
    }

    public class TeacherDashboardViewModel
    {
        public Teacher? Teacher { get; set; }
        public int AssignedClassCount { get; set; }
        public int StudentCount { get; set; }
        public int PendingMarks { get; set; }
        public List<ExamSchedule> TodaySchedules { get; set; } = new();
        public List<AcademicCalendarEvent> Announcements { get; set; } = new();
    }

    public class TeacherClassesViewModel
    {
        public Teacher? Teacher { get; set; }
        public List<Class> Classes { get; set; } = new();
    }

    public class TeacherAttendanceViewModel
    {
        public int? ClassId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public List<SelectListItem> Classes { get; set; } = new();
        public List<AttendanceStudentRowViewModel> Students { get; set; } = new();
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int ExcusedCount { get; set; }
    }

    public class TeacherAttendanceSaveViewModel
    {
        public int ClassId { get; set; }
        public DateTime AttendanceDate { get; set; } = DateTime.Today;
        public List<TeacherAttendanceInputViewModel> Rows { get; set; } = new();
    }

    public class TeacherAttendanceInputViewModel
    {
        public int StudentId { get; set; }
        public string Status { get; set; } = "Present";
        public string Notes { get; set; } = string.Empty;
    }

    public class TeacherMarksViewModel
    {
        public int? ExamTermId { get; set; }
        public int? ClassId { get; set; }
        public int? SubjectId { get; set; }
        public List<SelectListItem> Terms { get; set; } = new();
        public List<SelectListItem> Classes { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
        public List<ResultStudentRowViewModel> Students { get; set; } = new();
    }

    public class LearningMaterialCreateViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? ClassId { get; set; }
        public int? SubjectId { get; set; }
        public string MaterialType { get; set; } = "Document";
        public IFormFile? File { get; set; }
    }

    public class MaterialsPageViewModel
    {
        public List<LearningMaterial> Materials { get; set; } = new();
        public List<SelectListItem> Classes { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
    }

    public class AssignmentCreateViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public int? SubjectId { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);
        public decimal MaxScore { get; set; } = 100;
    }

    public class AssignmentsPageViewModel
    {
        public List<Assignment> Assignments { get; set; } = new();
        public List<SelectListItem> Classes { get; set; } = new();
        public List<SelectListItem> Subjects { get; set; } = new();
    }

    public class StudentDashboardViewModel
    {
        public Student? Student { get; set; }
        public decimal FeeBalance { get; set; }
        public int SubjectCount { get; set; }
        public decimal AverageScore { get; set; }
        public List<ExamResult> RecentResults { get; set; } = new();
        public List<Notice> Notices { get; set; } = new();
    }

    public class StudentResultsViewModel
    {
        public Student? Student { get; set; }
        public List<SubjectResultSummaryViewModel> Subjects { get; set; } = new();
        public decimal Average { get; set; }
        public string Grade { get; set; } = "N/A";
    }

    public class SubjectResultSummaryViewModel
    {
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string Grade { get; set; } = "N/A";
        public string Status { get; set; } = "Missing";
    }

    public class StudentFeesViewModel
    {
        public Student? Student { get; set; }
        public List<FeeInvoice> Invoices { get; set; } = new();
        public List<Payment> Payments { get; set; } = new();
        public decimal Balance { get; set; }
    }

    public class StudentScheduleViewModel
    {
        public Student? Student { get; set; }
        public List<ExamSchedule> Schedules { get; set; } = new();
        public List<AcademicCalendarEvent> Events { get; set; } = new();
    }

    public class StudentAssignmentsViewModel
    {
        public Student? Student { get; set; }
        public List<Assignment> Assignments { get; set; } = new();
        public List<AssignmentSubmission> Submissions { get; set; } = new();
    }

    public class AssignmentSubmitViewModel
    {
        public int AssignmentId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public IFormFile? File { get; set; }
    }
}
