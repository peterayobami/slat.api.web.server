using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Slat.Api.Web.Server.Migrations
{
    public partial class StudentAccessCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccessCode",
                table: "Students",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessCode",
                table: "Students");
        }
    }
}
