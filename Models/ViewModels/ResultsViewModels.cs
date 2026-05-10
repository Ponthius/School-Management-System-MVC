using Microsoft.AspNetCore.Mvc.Rendering;

namespace School_Management_System.Models.ViewModels
{
    public class ResultsEntryViewModel
    {
        public int? ExamTermId { get; set; }
        public int? ClassId { get; set; }
        public int? SubjectId { get; set; }
        public string Search { get; set; } = string.Empty;
        public List<SelectListItem> ExamTermOptions { get; set; } = new();
        public List<SelectListItem> ClassOptions { get; set; } = new();
        public List<SelectListItem> SubjectOptions { get; set; } = new();
        public List<ResultStudentRowViewModel> Students { get; set; } = new();
        public int TotalStudents { get; set; }
        public decimal PassThreshold { get; set; } = 50;
        public decimal CurrentAverage { get; set; }
        public int MissingScores { get; set; }
        public string SelectedTermName { get; set; } = string.Empty;
        public string SelectedClassName { get; set; } = string.Empty;
        public string SelectedSubjectName { get; set; } = string.Empty;
    }

    public class ResultStudentRowViewModel
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string PhotoPath { get; set; } = "/images/default-avatar.png";
        public decimal? Score { get; set; }
        public string TeacherComments { get; set; } = string.Empty;
        public string Status { get; set; } = "Missing";
        public decimal Average { get; set; }
        public string Grade { get; set; } = "N/A";
    }

    public class ResultsSaveViewModel
    {
        public int ExamTermId { get; set; }
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public string Search { get; set; } = string.Empty;
        public List<ResultScoreInputViewModel> Scores { get; set; } = new();
    }

    public class ResultScoreInputViewModel
    {
        public int StudentId { get; set; }
        public decimal? Score { get; set; }
        public string TeacherComments { get; set; } = string.Empty;
    }
}
