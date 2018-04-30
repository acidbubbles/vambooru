using Microsoft.EntityFrameworkCore.Migrations;

namespace VamBooru.Migrations
{
	public partial class RequiredLoginInfo : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AlterColumn<string>(
				name: "Scheme",
				table: "UserLogins",
				nullable: false
			);

			migrationBuilder.AlterColumn<string>(
				name: "NameIdentifier",
				table: "UserLogins",
				nullable: false
			);

			migrationBuilder.AlterColumn<string>(
				name: "MimeType",
				table: "PostFiles",
				nullable: false
			);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
		}
	}
}
