using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReleasedColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Members_dbo.MemberStatuses_StatusId",
                table: "Members");

            migrationBuilder.DropTable(
                name: "UserTypes");

            migrationBuilder.DropIndex(
                name: "IX_Members_StatusId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "AccessFailedCount",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ConcurrencyStamp",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EmailConfirmed",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EmergencyPhoneNumber",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EmergencyRelation",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "NormalizedEmail",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "NormalizedUserName",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "UserName",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "TwoFactorEnabled",
                table: "Members");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleasedOn",
                table: "Members",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleasedOn",
                table: "Members");

            migrationBuilder.AddColumn<bool>(
                name: "TwoFactorEnabled",
                table: "Members",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AccessFailedCount",
                table: "Members",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyStamp",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "EmailConfirmed",
                table: "Members",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                table: "Members",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyPhoneNumber",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyRelation",
                table: "Members",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Members",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "Members",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedEmail",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedUserName",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "Members",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Members",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                table: "Members",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "UserTypes",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.MemberStatuses", x => x.StatusId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Members_StatusId",
                table: "Members",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Members_dbo.MemberStatuses_StatusId",
                table: "Members",
                column: "StatusId",
                principalTable: "UserTypes",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
