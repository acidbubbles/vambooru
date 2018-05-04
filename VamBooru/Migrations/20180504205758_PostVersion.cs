using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace VamBooru.Migrations
{
	public partial class PostVersion : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<int>(
				name: "Version",
				table: "Posts",
				nullable: false,
				defaultValue: 1);
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropColumn(
				name: "Version",
				table: "Posts");
		}
	}
}
