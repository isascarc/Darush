using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyJob.Migrations
{
    /// <inheritdoc />
    public partial class jobareaint2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Area",
                table: "Jobs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Area",
                table: "Jobs");
        }
    }
}
