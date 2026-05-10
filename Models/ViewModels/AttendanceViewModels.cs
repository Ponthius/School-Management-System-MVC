using Microsoft.AspNetCore.Mvc.Rendering;

namespace School_Management_System.Models.ViewModels
{
    public class AttendanceDashboardViewModel
    {
        public int? SelectedClassId { get; set; }
        public DateTime SelectedDate { get; set; } = DateTime.Today;
        public string Search { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public List<SelectListItem> ClassOptions { get; set; } = new();
        public List<AttendanceStudentRowViewModel> Students { get; set; } = new();
        public int TotalStudents { get; set; }
        public int PresentCount { get; set; }
        public int AbsentCount { get; set; }
        public int LateCount { get; set; }
        public int NotMarkedCount { get; set; }
    }

    public class AttendanceStudentRowViewModel
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = "/images/default-avatar.png";
        public string Status { get; set; } = "Not Marked";
        public string Notes { get; set; } = string.Empty;
        public string MarkedByName { get; set; } = string.Empty;
        public string MarkedBySource { get; set; } = string.Empty;
        public DateTime? MarkedAt { get; set; }
    }

    public class AttendanceStatusUpdateViewModel
    {
        public int StudentId { get; set; }
        public int ClassId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public string Status { get; set; } = "Present";
        public string Notes { get; set; } = string.Empty;
        public int? SelectedClassId { get; set; }
        public string Search { get; set; } = string.Empty;
    }
}
