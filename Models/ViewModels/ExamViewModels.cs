using Microsoft.AspNetCore.Mvc.Rendering;

namespace School_Management_System.Models.ViewModels
{
    public class ExamManagementViewModel
    {
        public string Search { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? SelectedTermId { get; set; }
        public decimal OverallAverage { get; set; }
        public decimal PassingRate { get; set; }
        public int TotalParticipants { get; set; }
        public int ActiveTermCount { get; set; }
        public List<ExamTermRowViewModel> Terms { get; set; } = new();
        public List<ExamScheduleRowViewModel> Schedules { get; set; } = new();
        public List<SelectListItem> TermOptions { get; set; } = new();
        public List<SelectListItem> ClassOptions { get; set; } = new();
        public List<SelectListItem> SubjectOptions { get; set; } = new();
        public List<SelectListItem> TeacherOptions { get; set; } = new();
    }

    public class ExamTermRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ScheduleCount { get; set; }
        public int ParticipantCount { get; set; }
    }

    public class ExamScheduleRowViewModel
    {
        public int Id { get; set; }
        public int ExamTermId { get; set; }
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int? InvigilatorTeacherId { get; set; }
        public string TermName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string InvigilatorName { get; set; } = "Unassigned";
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Notes { get; set; } = string.Empty;
    }

    public class ExamTermUpsertViewModel
    {
        public int? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        public string Status { get; set; } = "Upcoming";
    }

    public class ExamScheduleUpsertViewModel
    {
        public int? Id { get; set; }
        public int ExamTermId { get; set; }
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public int? InvigilatorTeacherId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public DateTime ExamDate { get; set; } = DateTime.Today;
        public TimeSpan StartTime { get; set; } = new(9, 0, 0);
        public TimeSpan EndTime { get; set; } = new(11, 0, 0);
        public string Notes { get; set; } = string.Empty;
    }

    public class ExamPrintTimetableViewModel
    {
        public string TermName { get; set; } = "All Exam Terms";
        public List<ExamScheduleRowViewModel> Schedules { get; set; } = new();
    }
}
