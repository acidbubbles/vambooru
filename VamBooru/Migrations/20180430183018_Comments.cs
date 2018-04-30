using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace VamBooru.Migrations
{
	public partial class Comments : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "PostComments",
				columns: table => new
				{
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					AuthorId = table.Column<Guid>(nullable: true),
					DateCreated = table.Column<DateTimeOffset>(nullable: false),
					PostId = table.Column<Guid>(nullable: true),
					Text = table.Column<string>(type: "text", nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PostComments", x => x.Id);
					table.ForeignKey(
						name: "FK_PostComments_Users_AuthorId",
						column: x => x.AuthorId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_PostComments_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_PostComments_AuthorId",
				table: "PostComments",
				column: "AuthorId");

			migrationBuilder.CreateIndex(
				name: "IX_PostComments_DateCreated",
				table: "PostComments",
				column: "DateCreated");

			migrationBuilder.CreateIndex(
				name: "IX_PostComments_PostId",
				table: "PostComments",
				column: "PostId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PostComments");
		}
	}
}
