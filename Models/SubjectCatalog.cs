namespace School_Management_System.Models
{
    public static class SubjectCatalog
    {
        public static readonly (int Id, string Code, string Name, string Department)[] SeedSubjects =
        {
            (1, "MAT-001", "Mathematics", "STEM"),
            (2, "PHY-002", "Physics", "STEM"),
            (3, "CHE-003", "Chemistry", "STEM"),
            (4, "BIO-004", "Biology", "STEM"),
            (5, "ENG-005", "English Literature", "Languages"),
            (6, "ENG-006", "English Language", "Languages"),
            (7, "HIS-007", "History", "Humanities"),
            (8, "GEO-008", "Geography", "Humanities"),
            (9, "SST-009", "Social Studies", "Humanities"),
            (10, "CSC-010", "Computer Science", "STEM"),
            (11, "PED-011", "Physical Education", "Arts"),
            (12, "ART-012", "Fine Art", "Arts"),
            (13, "MUS-013", "Music", "Arts"),
            (14, "ECO-014", "Economics", "Business"),
            (15, "COM-015", "Commerce", "Business"),
            (16, "AGR-016", "Agriculture", "STEM"),
            (17, "CRE-017", "CRE", "Humanities"),
            (18, "IRE-018", "IRE", "Humanities")
        };

        public static readonly string[] Names = SeedSubjects.Select(s => s.Name).ToArray();
    }
}
