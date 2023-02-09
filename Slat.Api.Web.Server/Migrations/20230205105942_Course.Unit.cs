using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slat.Api.Web.Server.Migrations
{
    public partial class CourseUnit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Lecturers_LecturerId",
                table: "Courses");

            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Courses_LecturerId",
                table: "Lectures");

            migrationBuilder.DropIndex(
                name: "IX_Courses_LecturerId",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LecturerId",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "CoursesDataModelId",
                table: "Lectures",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Unit",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LecturerCourses",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CourseId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LecturerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerCourses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LecturerCourses_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LecturerCourses_Lecturers_LecturerId",
                        column: x => x.LecturerId,
                        principalTable: "Lecturers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_CoursesDataModelId",
                table: "Lectures",
                column: "CoursesDataModelId");

            migrationBuilder.CreateIndex(
                name: "IX_LecturerCourses_CourseId",
                table: "LecturerCourses",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_LecturerCourses_LecturerId",
                table: "LecturerCourses",
                column: "LecturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Courses_CoursesDataModelId",
                table: "Lectures",
                column: "CoursesDataModelId",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Courses_CoursesDataModelId",
                table: "Lectures");

            migrationBuilder.DropTable(
                name: "LecturerCourses");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_CoursesDataModelId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "CoursesDataModelId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "Courses");

            migrationBuilder.AddColumn<string>(
                name: "LecturerId",
                table: "Courses",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_LecturerId",
                table: "Courses",
                column: "LecturerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Lecturers_LecturerId",
                table: "Courses",
                column: "LecturerId",
                principalTable: "Lecturers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Courses_LecturerId",
                table: "Lectures",
                column: "LecturerId",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
