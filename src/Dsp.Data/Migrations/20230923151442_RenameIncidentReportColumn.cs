using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameIncidentReportColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ReportedBy",
                table: "IncidentReports",
                newName: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReports_UserId",
                table: "IncidentReports",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IncidentReports_UserId",
                table: "IncidentReports");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "IncidentReports",
                newName: "ReportedBy");
        }
    }
}
