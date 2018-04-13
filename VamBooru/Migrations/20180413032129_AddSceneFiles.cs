using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace VamBooru.Migrations
{
	public partial class AddSceneFiles : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "SceneFiles",
				columns: table => new
				{
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Bytes = table.Column<byte[]>(nullable: true),
					Filename = table.Column<string>(nullable: true),
					SceneId = table.Column<Guid>(nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SceneFiles", x => x.Id);
					table.ForeignKey(
						name: "FK_SceneFiles_Scenes_SceneId",
						column: x => x.SceneId,
						principalTable: "Scenes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_SceneFiles_SceneId",
				table: "SceneFiles",
				column: "SceneId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SceneFiles");
		}
	}
}
