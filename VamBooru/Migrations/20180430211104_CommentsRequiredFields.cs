using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace VamBooru.Migrations
{
	public partial class CommentsRequiredFields : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_PostComments_Users_AuthorId",
				table: "PostComments");

			migrationBuilder.DropForeignKey(
				name: "FK_PostComments_Posts_PostId",
				table: "PostComments");

			migrationBuilder.AlterColumn<Guid>(
				name: "PostId",
				table: "PostComments",
				nullable: false,
				oldClrType: typeof(Guid),
				oldNullable: true);

			migrationBuilder.AlterColumn<Guid>(
				name: "AuthorId",
				table: "PostComments",
				nullable: false,
				oldClrType: typeof(Guid),
				oldNullable: true);

			migrationBuilder.AddForeignKey(
				name: "FK_PostComments_Users_AuthorId",
				table: "PostComments",
				column: "AuthorId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);

			migrationBuilder.AddForeignKey(
				name: "FK_PostComments_Posts_PostId",
				table: "PostComments",
				column: "PostId",
				principalTable: "Posts",
				principalColumn: "Id",
				onDelete: ReferentialAction.Cascade);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropForeignKey(
				name: "FK_PostComments_Users_AuthorId",
				table: "PostComments");

			migrationBuilder.DropForeignKey(
				name: "FK_PostComments_Posts_PostId",
				table: "PostComments");

			migrationBuilder.AlterColumn<Guid>(
				name: "PostId",
				table: "PostComments",
				nullable: true,
				oldClrType: typeof(Guid));

			migrationBuilder.AlterColumn<Guid>(
				name: "AuthorId",
				table: "PostComments",
				nullable: true,
				oldClrType: typeof(Guid));

			migrationBuilder.AddForeignKey(
				name: "FK_PostComments_Users_AuthorId",
				table: "PostComments",
				column: "AuthorId",
				principalTable: "Users",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);

			migrationBuilder.AddForeignKey(
				name: "FK_PostComments_Posts_PostId",
				table: "PostComments",
				column: "PostId",
				principalTable: "Posts",
				principalColumn: "Id",
				onDelete: ReferentialAction.Restrict);
		}
	}
}
