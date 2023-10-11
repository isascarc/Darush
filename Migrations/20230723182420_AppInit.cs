using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyJob.Migrations;

/// <inheritdoc />
public partial class AppInit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Recruiters",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                RecName = table.Column<string>(type: "TEXT", nullable: true),
                PasswordHash = table.Column<byte[]>(type: "BLOB", nullable: true),
                PasswordSalt = table.Column<byte[]>(type: "BLOB", nullable: true),
                Create = table.Column<DateTime>(type: "TEXT", nullable: false),
                LastActive = table.Column<DateTime>(type: "TEXT", nullable: false),
                Gender = table.Column<string>(type: "TEXT", nullable: true),
                InShort = table.Column<string>(type: "TEXT", nullable: true),
                LogoProfile = table.Column<string>(type: "TEXT", nullable: true),
                City = table.Column<string>(type: "TEXT", nullable: true),
                Mail = table.Column<string>(type: "TEXT", nullable: true),
                Phone = table.Column<string>(type: "TEXT", nullable: true),
                LinkedinLink = table.Column<string>(type: "TEXT", nullable: true),
                Deleted = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Recruiters", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                UserName = table.Column<string>(type: "TEXT", nullable: true),
                PasswordHash = table.Column<byte[]>(type: "BLOB", nullable: true),
                PasswordSalt = table.Column<byte[]>(type: "BLOB", nullable: true),
                DateOfBirth = table.Column<DateOnly>(type: "TEXT", nullable: false),
                KnownAs = table.Column<string>(type: "TEXT", nullable: true),
                Create = table.Column<DateTime>(type: "TEXT", nullable: false),
                LastActive = table.Column<DateTime>(type: "TEXT", nullable: false),
                Gender = table.Column<string>(type: "TEXT", nullable: true),
                City = table.Column<string>(type: "TEXT", nullable: true),
                Mail = table.Column<string>(type: "TEXT", nullable: true),
                Phone = table.Column<string>(type: "TEXT", nullable: true),
                LinkedinLink = table.Column<string>(type: "TEXT", nullable: true),
                WebsiteLink = table.Column<string>(type: "TEXT", nullable: true),
                Deleted = table.Column<bool>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Jobs",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                text = table.Column<string>(type: "TEXT", nullable: true),
                DateOfAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                salary = table.Column<int>(type: "INTEGER", nullable: false),
                haveToar = table.Column<bool>(type: "INTEGER", nullable: false),
                EnglishNeed = table.Column<bool>(type: "INTEGER", nullable: false),
                Found = table.Column<bool>(type: "INTEGER", nullable: false),
                FoundDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                RecruiterId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Jobs", x => x.Id);
                table.ForeignKey(
                    name: "FK_Jobs_Recruiters_RecruiterId",
                    column: x => x.RecruiterId,
                    principalTable: "Recruiters",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "CVs",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Text = table.Column<string>(type: "TEXT", nullable: true),
                FileContent = table.Column<byte[]>(type: "BLOB", nullable: true),
                DateOfAdded = table.Column<DateTime>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", nullable: true),
                IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                Deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                AppUserId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CVs", x => x.Id);
                table.ForeignKey(
                    name: "FK_CVs_Users_AppUserId",
                    column: x => x.AppUserId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Applicants",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                Create = table.Column<DateTime>(type: "TEXT", nullable: false),
                LinkedinLink = table.Column<string>(type: "TEXT", nullable: true),
                CvId = table.Column<int>(type: "INTEGER", nullable: false),
                UserId = table.Column<int>(type: "INTEGER", nullable: false),
                JobId = table.Column<int>(type: "INTEGER", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Applicants", x => x.Id);
                table.ForeignKey(
                    name: "FK_Applicants_Jobs_JobId",
                    column: x => x.JobId,
                    principalTable: "Jobs",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Applicants_JobId",
            table: "Applicants",
            column: "JobId");

        migrationBuilder.CreateIndex(
            name: "IX_CVs_AppUserId",
            table: "CVs",
            column: "AppUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Jobs_RecruiterId",
            table: "Jobs",
            column: "RecruiterId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Applicants");

        migrationBuilder.DropTable(
            name: "CVs");

        migrationBuilder.DropTable(
            name: "Jobs");

        migrationBuilder.DropTable(
            name: "Users");

        migrationBuilder.DropTable(
            name: "Recruiters");
    }
}
