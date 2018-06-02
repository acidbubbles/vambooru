using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;

namespace VamBooru.Migrations
{
	public partial class SupportFiles : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
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
				name: "IX_SupportFiles_Filename",
				table: "SupportFiles",
				column: "Filename");

			migrationBuilder.CreateIndex(
				name: "IX_SupportFiles_PostId",
				table: "SupportFiles",
				column: "PostId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SupportFiles");
		}
	}
}
