using Microsoft.EntityFrameworkCore.Metadata;
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
				name: "SceneTag",
				columns: table => new
				{
					Id = table.Column<long>(nullable: false)
						.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
					SceneId = table.Column<Guid>(nullable: true),
					TagId = table.Column<Guid>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SceneTag", x => x.Id);
					table.ForeignKey(
						name: "FK_SceneTag_Scenes_SceneId",
						column: x => x.SceneId,
						principalTable: "Scenes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_SceneTag_Tags_TagId",
						column: x => x.TagId,
						principalTable: "Tags",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_Scenes_AuthorId",
				table: "Scenes",
				column: "AuthorId");

			migrationBuilder.CreateIndex(
				name: "IX_SceneTag_SceneId",
				table: "SceneTag",
				column: "SceneId");

			migrationBuilder.CreateIndex(
				name: "IX_SceneTag_TagId",
				table: "SceneTag",
				column: "TagId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SceneTag");

			migrationBuilder.DropTable(
				name: "Scenes");

			migrationBuilder.DropTable(
				name: "Tags");

			migrationBuilder.DropTable(
				name: "Authors");
		}
	}
}
