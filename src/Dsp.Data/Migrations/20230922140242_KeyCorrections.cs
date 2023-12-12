using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class KeyCorrections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceAmendments_dbo.Semesters_SemesterId",
                table: "ServiceEventAmendments");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHourAmendments_SemesterId",
                table: "ServiceHourAmendments",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHourAmendments_UserId",
                table: "ServiceHourAmendments",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Semesters_SemesterId",
                table: "ServiceEventAmendments",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Members_UserId",
                table: "ServiceHourAmendments",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Semesters_SemesterId",
                table: "ServiceHourAmendments",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Semesters_SemesterId",
                table: "ServiceEventAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Members_UserId",
                table: "ServiceHourAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Semesters_SemesterId",
                table: "ServiceHourAmendments");

            migrationBuilder.DropIndex(
                name: "IX_ServiceHourAmendments_SemesterId",
                table: "ServiceHourAmendments");

            migrationBuilder.DropIndex(
                name: "IX_ServiceHourAmendments_UserId",
                table: "ServiceHourAmendments");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceAmendments_dbo.Semesters_SemesterId",
                table: "ServiceEventAmendments",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "SemesterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
