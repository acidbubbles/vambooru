using Microsoft.EntityFrameworkCore.Migrations;

namespace VamBooru.Migrations
{
	public partial class UserRole : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "Role",
				table: "Users",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.AlterColumn<string>(
				name: "MimeType",
				table: "PostFiles",
				nullable: false,
				oldClrType: typeof(string),
				oldNullable: true);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Role",
				table: "Users");

			migrationBuilder.AlterColumn<string>(
				name: "MimeType",
				table: "PostFiles",
				nullable: true,
				oldClrType: typeof(string));
		}
	}
}
