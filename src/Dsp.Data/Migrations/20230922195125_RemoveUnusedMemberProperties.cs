using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedMemberProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LockoutEndDateUtc",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Pin",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ShirtSize",
                table: "Members");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndDateUtc",
                table: "Members",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Pin",
                table: "Members",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShirtSize",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
