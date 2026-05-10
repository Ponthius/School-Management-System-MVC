using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace School_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeededClassSubjectData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @TeacherUserIds TABLE ([Id] int NOT NULL PRIMARY KEY);

                INSERT INTO @TeacherUserIds ([Id])
                SELECT [UserId]
                FROM [Teachers];

                UPDATE [Classes]
                SET [TeacherId] = NULL;

                DELETE FROM [Teachers];

                DELETE FROM [Users]
                WHERE [Role] = N'Teacher'
                   OR [Id] IN (SELECT [Id] FROM @TeacherUserIds);

                DELETE FROM [ClassSubjects];

                DELETE FROM [Subjects];

                DELETE c
                FROM [Classes] c
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [Students] s
                    WHERE s.[ClassId] = c.[Id]
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "Capacity", "Name", "TeacherId" },
                values: new object[,]
                {
                    { 1, 40, "S1-A", null },
                    { 2, 40, "S1-B", null },
                    { 3, 40, "S2-A", null },
                    { 4, 40, "S2-B", null },
                    { 5, 40, "S3-A", null },
                    { 6, 40, "S3-B", null },
                    { 7, 45, "P1", null },
                    { 8, 45, "P2", null },
                    { 9, 45, "P3", null },
                    { 10, 45, "P4", null },
                    { 11, 45, "P5", null },
                    { 12, 45, "P6", null },
                    { 13, 45, "P7", null }
                });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Code", "Department", "Name" },
                values: new object[,]
                {
                    { 1, "MAT-001", "STEM", "Mathematics" },
                    { 2, "PHY-002", "STEM", "Physics" },
                    { 3, "CHE-003", "STEM", "Chemistry" },
                    { 4, "BIO-004", "STEM", "Biology" },
                    { 5, "ENG-005", "Languages", "English Literature" },
                    { 6, "ENG-006", "Languages", "English Language" },
                    { 7, "HIS-007", "Humanities", "History" },
                    { 8, "GEO-008", "Humanities", "Geography" },
                    { 9, "SST-009", "Humanities", "Social Studies" },
                    { 10, "CSC-010", "STEM", "Computer Science" },
                    { 11, "PED-011", "Arts", "Physical Education" },
                    { 12, "ART-012", "Arts", "Fine Art" },
                    { 13, "MUS-013", "Arts", "Music" },
                    { 14, "ECO-014", "Business", "Economics" },
                    { 15, "COM-015", "Business", "Commerce" },
                    { 16, "AGR-016", "STEM", "Agriculture" },
                    { 17, "CRE-017", "Humanities", "CRE" },
                    { 18, "IRE-018", "Humanities", "IRE" }
                });
        }
    }
}
