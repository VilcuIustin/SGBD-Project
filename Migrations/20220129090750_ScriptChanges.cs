using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGBD_Project.Migrations
{
    public partial class ScriptChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Scripts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Query",
                table: "Scripts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Scripts_DateCreated",
                table: "Scripts",
                column: "DateCreated");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Scripts_DateCreated",
                table: "Scripts");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Scripts");

            migrationBuilder.DropColumn(
                name: "Query",
                table: "Scripts");
        }
    }
}
