using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace School_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class PersonalPcSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClassSubjects",
                columns: table => new
                {
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSubjects", x => new { x.ClassId, x.SubjectId });
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_SubjectId",
                table: "ClassSubjects",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassSubjects");

            migrationBuilder.DropTable(
                name: "Subjects");
        }
    }
}
