using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace VamBooru.Migrations
{
	public partial class Votes : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "Votes",
				table: "Posts",
				nullable: false,
				defaultValue: 0);

			migrationBuilder.CreateTable(
				name: "UserPostVote",
				columns: table => new
				{
					UserId = table.Column<Guid>(nullable: false),
					PostId = table.Column<Guid>(nullable: false),
					Votes = table.Column<int>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserPostVote", x => new { x.UserId, x.PostId });
					table.ForeignKey(
						name: "FK_UserPostVote_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_UserPostVote_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_UserPostVote_PostId",
				table: "UserPostVote",
				column: "PostId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "UserPostVote");

			migrationBuilder.DropColumn(
				name: "Votes",
				table: "Posts");
		}
	}
}
