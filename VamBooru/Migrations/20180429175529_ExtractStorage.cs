using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace VamBooru.Migrations
{
	public partial class ExtractStorage : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "ImageUrl",
				table: "Posts",
				newName: "ThumbnailUrn");

			migrationBuilder.AddColumn<string>(
				name: "ThumbnailUrn",
				table: "Scenes",
				nullable: true);

			migrationBuilder.AlterColumn<DateTimeOffset>(
				name: "DatePublished",
				table: "Posts",
				nullable: true,
				oldClrType: typeof(DateTimeOffset));

			migrationBuilder.CreateTable(
				name: "PostFiles",
				columns: table => new
				{
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Compressed = table.Column<bool>(nullable: false),
					Filename = table.Column<string>(nullable: false),
					MimeType = table.Column<string>(nullable: true),
					PostId = table.Column<Guid>(nullable: false),
					Urn = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PostFiles", x => x.Id);
					table.ForeignKey(
						name: "FK_PostFiles_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "StorageFiles",
				columns: table => new
				{
					Id = table.Column<long>(nullable: false)
						.Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
					Bytes = table.Column<byte[]>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_StorageFiles", x => x.Id);
				});

			migrationBuilder.CreateIndex(
				name: "IX_PostFiles_PostId",
				table: "PostFiles",
				column: "PostId");

			migrationBuilder.CreateIndex(
				name: "IX_PostFiles_Urn",
				table: "PostFiles",
				column: "Urn",
				unique: true);


			// Migrate data

			migrationBuilder.Sql("UPDATE \"Posts\" SET \"DatePublished\" = NULL WHERE \"DatePublished\" = '0001-12-31 19:03:58-04:56:02 BC'");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PostFiles");

			migrationBuilder.DropTable(
				name: "StorageFiles");

			migrationBuilder.DropColumn(
				name: "ThumbnailUrn",
				table: "Scenes");

			migrationBuilder.RenameColumn(
				name: "ThumbnailUrn",
				table: "Posts",
				newName: "ImageUrl");

			migrationBuilder.AlterColumn<DateTimeOffset>(
				name: "DatePublished",
				table: "Posts",
				nullable: false,
				oldClrType: typeof(DateTimeOffset),
				oldNullable: true);

		}
	}
}
