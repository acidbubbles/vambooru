using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace VamBooru.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Authors",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Username = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Authors", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Tags",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Name = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tags", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Scenes",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					AuthorId = table.Column<Guid>(nullable: true),
					ImageUrl = table.Column<string>(nullable: true),
					Published = table.Column<bool>(nullable: false),
					Title = table.Column<string>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Scenes", x => x.Id);
					table.ForeignKey(
						name: "FK_Scenes_Authors_AuthorId",
						column: x => x.AuthorId,
						principalTable: "Authors",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateTable(
				name: "SceneTags",
				columns: table => new
				{
					SceneId = table.Column<Guid>(nullable: false),
					TagId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SceneTags", x => new { x.SceneId, x.TagId });
					table.ForeignKey(
						name: "FK_SceneTags_Scenes_SceneId",
						column: x => x.SceneId,
						principalTable: "Scenes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_SceneTags_Tags_TagId",
						column: x => x.TagId,
						principalTable: "Tags",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Scenes_AuthorId",
				table: "Scenes",
				column: "AuthorId");

			migrationBuilder.CreateIndex(
				name: "IX_SceneTags_TagId",
				table: "SceneTags",
				column: "TagId");

			migrationBuilder.CreateIndex(
				name: "IX_Tags_Name",
				table: "Tags",
				column: "Name",
				unique: true,
				filter: "[Name] IS NOT NULL");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SceneTags");

			migrationBuilder.DropTable(
				name: "Scenes");

			migrationBuilder.DropTable(
				name: "Tags");

			migrationBuilder.DropTable(
				name: "Authors");
		}
	}
}
