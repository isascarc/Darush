using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyJob.Migrations
{
    /// <inheritdoc />
    public partial class FixSalary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "salary",
                table: "Jobs",
                newName: "Salary");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Salary",
                table: "Jobs",
                newName: "salary");
        }
    }
}
