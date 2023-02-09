using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slat.Api.Web.Server.Migrations
{
    public partial class LectureCourse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Courses_CoursesDataModelId",
                table: "Lectures");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_CoursesDataModelId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "CoursesDataModelId",
                table: "Lectures");

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Lectures",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_CourseId",
                table: "Lectures",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Courses_CourseId",
                table: "Lectures",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Courses_CourseId",
                table: "Lectures");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_CourseId",
                table: "Lectures");

            migrationBuilder.AlterColumn<string>(
                name: "CourseId",
                table: "Lectures",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CoursesDataModelId",
                table: "Lectures",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_CoursesDataModelId",
                table: "Lectures",
                column: "CoursesDataModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Courses_CoursesDataModelId",
                table: "Lectures",
                column: "CoursesDataModelId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
