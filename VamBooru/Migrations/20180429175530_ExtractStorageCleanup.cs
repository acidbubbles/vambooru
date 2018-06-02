using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace VamBooru.Migrations
{
	public partial class ExtractStorageCleanup : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			// Avoid running this migration if /api/admin/migrations/ExtractStorage was not run)
			migrationBuilder.Sql(@"DO language plpgsql $$
BEGIN
	IF (SELECT COUNT(""Id"") FROM ""SceneFiles"") > 0 THEN
		RAISE EXCEPTION 'Manual files migration was not yet complete';
	END IF;
END
$$;");
			migrationBuilder.DropTable(
				name: "SceneFiles");

			migrationBuilder.DropTable(
				name: "SupportFiles");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "SceneFiles",
				columns: table => new
				{
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Bytes = table.Column<byte[]>(nullable: false),
					Extension = table.Column<string>(nullable: false),
					Filename = table.Column<string>(nullable: false),
					SceneId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SceneFiles", x => x.Id);
					table.ForeignKey(
						name: "FK_SceneFiles_Scenes_SceneId",
						column: x => x.SceneId,
						principalTable: "Scenes",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "SupportFiles",
				columns: table => new
				{
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Bytes = table.Column<byte[]>(nullable: false),
					Filename = table.Column<string>(nullable: false),
					PostId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SupportFiles", x => x.Id);
					table.ForeignKey(
						name: "FK_SupportFiles_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateIndex(
				name: "IX_SceneFiles_Extension",
				table: "SceneFiles",
				column: "Extension");

			migrationBuilder.CreateIndex(
				name: "IX_SceneFiles_Filename",
				table: "SceneFiles",
				column: "Filename");

			migrationBuilder.CreateIndex(
				name: "IX_SceneFiles_SceneId",
				table: "SceneFiles",
				column: "SceneId");

			migrationBuilder.CreateIndex(
				name: "IX_SupportFiles_Filename",
				table: "SupportFiles",
				column: "Filename");

			migrationBuilder.CreateIndex(
				name: "IX_SupportFiles_PostId",
				table: "SupportFiles",
				column: "PostId");
		}
	}
}
