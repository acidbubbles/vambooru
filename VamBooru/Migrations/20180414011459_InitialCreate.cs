using Microsoft.EntityFrameworkCore.Migrations;
using System;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace VamBooru.Migrations
{
	public partial class InitialCreate : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable(
				name: "Tags",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Name = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Tags", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Users",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					DateSubscribed = table.Column<DateTimeOffset>(nullable: false),
					Username = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Users", x => x.Id);
				});

			migrationBuilder.CreateTable(
				name: "Posts",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					AuthorId = table.Column<Guid>(nullable: false),
					DateCreated = table.Column<DateTimeOffset>(nullable: false),
					DatePublished = table.Column<DateTimeOffset>(nullable: false),
					ImageUrl = table.Column<string>(nullable: true),
					Published = table.Column<bool>(nullable: false),
					Title = table.Column<string>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Posts", x => x.Id);
					table.ForeignKey(
						name: "FK_Posts_Users_AuthorId",
						column: x => x.AuthorId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "UserLogins",
				columns: table => new
				{
					Scheme = table.Column<string>(nullable: false),
					NameIdentifier = table.Column<string>(nullable: false),
					UserId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_UserLogins", x => new { x.Scheme, x.NameIdentifier });
					table.ForeignKey(
						name: "FK_UserLogins_Users_UserId",
						column: x => x.UserId,
						principalTable: "Users",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "PostTags",
				columns: table => new
				{
					PostId = table.Column<Guid>(nullable: false),
					TagId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_PostTags", x => new { x.PostId, x.TagId });
					table.ForeignKey(
						name: "FK_PostTags_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
					table.ForeignKey(
						name: "FK_PostTags_Tags_TagId",
						column: x => x.TagId,
						principalTable: "Tags",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

			migrationBuilder.CreateTable(
				name: "Scenes",
				columns: table => new
				{
					Id = table.Column<Guid>(nullable: false),
					Name = table.Column<string>(nullable: false),
					PostId = table.Column<Guid>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_Scenes", x => x.Id);
					table.ForeignKey(
						name: "FK_Scenes_Posts_PostId",
						column: x => x.PostId,
						principalTable: "Posts",
						principalColumn: "Id",
						onDelete: ReferentialAction.Cascade);
				});

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

			migrationBuilder.CreateIndex(
				name: "IX_Posts_AuthorId",
				table: "Posts",
				column: "AuthorId");

			migrationBuilder.CreateIndex(
				name: "IX_PostTags_TagId",
				table: "PostTags",
				column: "TagId");

			migrationBuilder.CreateIndex(
				name: "IX_SceneFiles_SceneId",
				table: "SceneFiles",
				column: "SceneId");

			migrationBuilder.CreateIndex(
				name: "IX_Scenes_PostId",
				table: "Scenes",
				column: "PostId");

			migrationBuilder.CreateIndex(
				name: "IX_Tags_Name",
				table: "Tags",
				column: "Name",
				unique: true);

			migrationBuilder.CreateIndex(
				name: "IX_UserLogins_UserId",
				table: "UserLogins",
				column: "UserId");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "PostTags");

			migrationBuilder.DropTable(
				name: "SceneFiles");

			migrationBuilder.DropTable(
				name: "UserLogins");

			migrationBuilder.DropTable(
				name: "Tags");

			migrationBuilder.DropTable(
				name: "Scenes");

			migrationBuilder.DropTable(
				name: "Posts");

			migrationBuilder.DropTable(
				name: "Users");
		}
	}
}
