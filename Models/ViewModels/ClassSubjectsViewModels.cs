using System.ComponentModel.DataAnnotations;

namespace School_Management_System.Models.ViewModels
{
    public class ClassSubjectsDashboardViewModel
    {
        public List<ClassSubjectsClassCardViewModel> ClassCards { get; set; } = new();
        public List<ClassSubjectsSubjectRowViewModel> SubjectRows { get; set; } = new();
        public List<ClassSubjectsStatViewModel> Stats { get; set; } = new();
        public string Search { get; set; } = string.Empty;
    }

    public class ClassSubjectsClassCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int StudentCount { get; set; }
        public int SubjectCount { get; set; }
        public string ClassTeacherName { get; set; } = "Unassigned";
        public int? TeacherId { get; set; }
        public List<ClassSubjectsSubjectChipViewModel> Subjects { get; set; } = new();
        public string AccentClass { get; set; } = "accent-indigo";
    }

    public class ClassSubjectsSubjectChipViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class ClassSubjectsSubjectRowViewModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public List<string> ActiveClasses { get; set; } = new();
    }

    public class ClassSubjectsStatViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string AccentClass { get; set; } = string.Empty;
    }

    public class ClassUpsertViewModel
    {
        public int? Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Capacity { get; set; } = 40;
    }

    public class SubjectUpsertViewModel
    {
        public int? Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Department { get; set; } = string.Empty;
    }

    public class ClassTeacherAssignViewModel
    {
        [Required]
        public int ClassId { get; set; }

        public int? TeacherId { get; set; }
    }

    public class ClassSubjectAssignViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SubjectId { get; set; }
    }

    public class ClassSubjectRemoveViewModel
    {
        [Required]
        public int ClassId { get; set; }

        [Required]
        public int SubjectId { get; set; }
    }
}
