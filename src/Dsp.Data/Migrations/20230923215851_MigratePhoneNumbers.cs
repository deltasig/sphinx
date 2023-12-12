using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigratePhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.Sql(@"
            UPDATE M
            SET M.PhoneNumber = (
                SELECT TOP 1 P.PhoneNumber
                FROM PhoneNumbers as P
                WHERE P.UserId = M.UserId AND P.Type = 'Mobile'
            )
            FROM Members as M");

            migrationBuilder.Sql(@"
            UPDATE M
            SET M.EmergencyPhoneNumber = (
                SELECT TOP 1 P.PhoneNumber
                FROM PhoneNumbers as P
                WHERE P.UserId = M.UserId AND P.Type = 'Emergency'
            )
            FROM Members as M");

            migrationBuilder.DropTable(
                name: "PhoneNumbers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmergencyContact",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EmergencyPhoneNumber",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "EmergencyRelation",
                table: "Members");

            migrationBuilder.CreateTable(
                name: "PhoneNumbers",
                columns: table => new
                {
                    PhoneNumberId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.PhoneNumbers", x => x.PhoneNumberId);
                    table.ForeignKey(
                        name: "FK_dbo.PhoneNumbers_dbo.Members_UserId",
                        column: x => x.UserId,
                        principalTable: "Members",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhoneNumbers_UserId",
                table: "PhoneNumbers",
                column: "UserId");
        }
    }
}
