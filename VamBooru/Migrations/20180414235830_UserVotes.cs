using Microsoft.EntityFrameworkCore.Migrations;

namespace VamBooru.Migrations
{
	public partial class UserVotes : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_UserPostVote_Posts_PostId",
				table: "UserPostVote");

			migrationBuilder.DropForeignKey(
				name: "FK_UserPostVote_Users_UserId",
				table: "UserPostVote");

			migrationBuilder.DropPrimaryKey(
				name: "PK_UserPostVote",
				table: "UserPostVote");

			migrationBuilder.RenameTable(
				name: "UserPostVote",
				newName: "UserPostVotes");

			migrationBuilder.RenameIndex(
				name: "IX_UserPostVote_PostId",
				table: "UserPostVotes",
				newName: "IX_UserPostVotes_PostId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_UserPostVotes",
				table: "UserPostVotes",
				columns: new[] { "UserId", "PostId" });

			migrationBuilder.AddForeignKey(
				name: "FK_UserPostVotes_Posts_PostId",
				table: "UserPostVotes",
				column: "PostId",
				principalTable: "Posts",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_UserPostVotes_Users_UserId",
				table: "UserPostVotes",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_UserPostVotes_Posts_PostId",
				table: "UserPostVotes");

			migrationBuilder.DropForeignKey(
				name: "FK_UserPostVotes_Users_UserId",
				table: "UserPostVotes");

			migrationBuilder.DropPrimaryKey(
				name: "PK_UserPostVotes",
				table: "UserPostVotes");

			migrationBuilder.RenameTable(
				name: "UserPostVotes",
				newName: "UserPostVote");

			migrationBuilder.RenameIndex(
				name: "IX_UserPostVotes_PostId",
				table: "UserPostVote",
				newName: "IX_UserPostVote_PostId");

			migrationBuilder.AddPrimaryKey(
				name: "PK_UserPostVote",
				table: "UserPostVote",
				columns: new[] { "UserId", "PostId" });

			migrationBuilder.AddForeignKey(
				name: "FK_UserPostVote_Posts_PostId",
				table: "UserPostVote",
				column: "PostId",
				principalTable: "Posts",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_UserPostVote_Users_UserId",
				table: "UserPostVote",
				column: "UserId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}
	}
}
