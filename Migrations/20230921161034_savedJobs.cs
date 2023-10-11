using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyJob.Migrations;

/// <inheritdoc />
public partial class savedJobs : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AlterColumn<string>(
            name: "UserName",
            table: "Users",
            type: "TEXT",
            maxLength: 8,
            nullable: false,
            defaultValue: "",
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldNullable: true);

        migrationBuilder.AlterColumn<byte[]>(
            name: "PasswordSalt",
            table: "Users",
            type: "BLOB",
            nullable: false,
            defaultValue: new byte[0],
            oldClrType: typeof(byte[]),
            oldType: "BLOB",
            oldNullable: true);

        migrationBuilder.AlterColumn<byte[]>(
            name: "PasswordHash",
            table: "Users",
            type: "BLOB",
            nullable: false,
            defaultValue: new byte[0],
            oldClrType: typeof(byte[]),
            oldType: "BLOB",
            oldNullable: true);

        migrationBuilder.AddColumn<string>(
            name: "SavedJobs",
            table: "Users",
            type: "TEXT",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "SavedJobs",
            table: "Users");

        migrationBuilder.AlterColumn<string>(
            name: "UserName",
            table: "Users",
            type: "TEXT",
            nullable: true,
            oldClrType: typeof(string),
            oldType: "TEXT",
            oldMaxLength: 8);

        migrationBuilder.AlterColumn<byte[]>(
            name: "PasswordSalt",
            table: "Users",
            type: "BLOB",
            nullable: true,
            oldClrType: typeof(byte[]),
            oldType: "BLOB");

        migrationBuilder.AlterColumn<byte[]>(
            name: "PasswordHash",
            table: "Users",
            type: "BLOB",
            nullable: true,
            oldClrType: typeof(byte[]),
            oldType: "BLOB");
    }
}
