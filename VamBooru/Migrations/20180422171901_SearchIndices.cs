using Microsoft.EntityFrameworkCore.Migrations;

namespace VamBooru.Migrations
{
	public partial class SearchIndices : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateIndex(
					name: "IX_Users_Username",
					table: "Users",
					column: "Username",
					unique: true);

			migrationBuilder.CreateIndex(
					name: "IX_Tags_PostsCount",
					table: "Tags",
					column: "PostsCount");

			migrationBuilder.CreateIndex(
					name: "IX_SceneFiles_Extension",
					table: "SceneFiles",
					column: "Extension");

			migrationBuilder.CreateIndex(
					name: "IX_Posts_DateCreated",
					table: "Posts",
					column: "DateCreated");

			migrationBuilder.CreateIndex(
					name: "IX_Posts_DatePublished",
					table: "Posts",
					column: "DatePublished");

			migrationBuilder.CreateIndex(
					name: "IX_Posts_Votes",
					table: "Posts",
					column: "Votes");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropIndex(
					name: "IX_Users_Username",
					table: "Users");

			migrationBuilder.DropIndex(
					name: "IX_Tags_PostsCount",
					table: "Tags");

			migrationBuilder.DropIndex(
					name: "IX_SceneFiles_Extension",
					table: "SceneFiles");

			migrationBuilder.DropIndex(
					name: "IX_Posts_DateCreated",
					table: "Posts");

			migrationBuilder.DropIndex(
					name: "IX_Posts_DatePublished",
					table: "Posts");

			migrationBuilder.DropIndex(
					name: "IX_Posts_Votes",
					table: "Posts");
		}
	}
}
