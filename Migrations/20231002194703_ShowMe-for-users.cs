using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyJob.Migrations
{
    /// <inheritdoc />
    public partial class ShowMeforusers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ShowMe",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShowMe",
                table: "Users");
        }
    }
}
