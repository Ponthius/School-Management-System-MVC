using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace School_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignedSubjects",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Teachers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PrimaryClassId",
                table: "Teachers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Teachers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.CreateIndex(
                name: "IX_Teachers_PrimaryClassId",
                table: "Teachers",
                column: "PrimaryClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teachers_Classes_PrimaryClassId",
                table: "Teachers",
                column: "PrimaryClassId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teachers_Classes_PrimaryClassId",
                table: "Teachers");

            migrationBuilder.DropIndex(
                name: "IX_Teachers_PrimaryClassId",
                table: "Teachers");

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Classes",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DropColumn(
                name: "AssignedSubjects",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PrimaryClassId",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Teachers");
        }
    }
}
