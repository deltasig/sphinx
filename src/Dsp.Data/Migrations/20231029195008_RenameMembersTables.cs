using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameMembersTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Addresses_dbo.Members_UserId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ClassesTaken_dbo.Members_UserId",
                table: "ClassesTaken");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.IncidentReports_dbo.Members_ReportedBy",
                table: "IncidentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.LaundrySignups_dbo.Members_UserId",
                table: "LaundrySignups");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.MajorToMembers_dbo.Members_UserId",
                table: "MajorToMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.MealVotes_dbo.Members_UserId",
                table: "MealItemVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.MealLatePlates_dbo.Members_UserId",
                table: "MealPlates");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.RoomToMembers_dbo.Members_UserId",
                table: "RoomToMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Events_dbo.Members_SubmitterId",
                table: "ServiceEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Members_UserId",
                table: "ServiceHourAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceHours_dbo.Members_UserId",
                table: "ServiceHours");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.SoberSignups_dbo.Members_UserId",
                table: "SoberSignups");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders");

            migrationBuilder.RenameTable(
                name: "UserRoles",
                newName: "MembersPositions");

            migrationBuilder.RenameTable(
                name: "Roles",
                newName: "Positions");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Members");

            migrationBuilder.CreateIndex(
                name: "IX_Members_BigBroId",
                table: "Members",
                column: "BigBroId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_ExpectedGraduationId",
                table: "Members",
                column: "ExpectedGraduationId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_PledgeClassId",
                table: "Members",
                column: "PledgeClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_StatusId",
                table: "Members",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MembersPositions_LeaderId",
                table: "MembersPositions",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_MembersPositions_SemesterId",
                table: "MembersPositions",
                column: "SemesterId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Addresses_dbo.Members_UserId",
                table: "Addresses",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ClassesTaken_dbo.Members_UserId",
                table: "ClassesTaken",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.IncidentReports_dbo.Members_ReportedBy",
                table: "IncidentReports",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.LaundrySignups_dbo.Members_UserId",
                table: "LaundrySignups",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.MajorToMembers_dbo.Members_UserId",
                table: "MajorToMembers",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.MealVotes_dbo.Members_UserId",
                table: "MealItemVotes",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.MealLatePlates_dbo.Members_UserId",
                table: "MealPlates",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.RoomToMembers_dbo.Members_UserId",
                table: "RoomToMembers",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Events_dbo.Members_SubmitterId",
                table: "ServiceEvents",
                column: "SubmitterId",
                principalTable: "Members",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Members_UserId",
                table: "ServiceHourAmendments",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceHours_dbo.Members_UserId",
                table: "ServiceHours",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.SoberSignups_dbo.Members_UserId",
                table: "SoberSignups",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Addresses_dbo.Members_UserId",
                table: "Addresses");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ClassesTaken_dbo.Members_UserId",
                table: "ClassesTaken");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.IncidentReports_dbo.Members_ReportedBy",
                table: "IncidentReports");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.LaundrySignups_dbo.Members_UserId",
                table: "LaundrySignups");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.MajorToMembers_dbo.Members_UserId",
                table: "MajorToMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.MealVotes_dbo.Members_UserId",
                table: "MealItemVotes");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.MealLatePlates_dbo.Members_UserId",
                table: "MealPlates");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.RoomToMembers_dbo.Members_UserId",
                table: "RoomToMembers");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.Events_dbo.Members_SubmitterId",
                table: "ServiceEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Members_UserId",
                table: "ServiceHourAmendments");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.ServiceHours_dbo.Members_UserId",
                table: "ServiceHours");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.SoberSignups_dbo.Members_UserId",
                table: "SoberSignups");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders");

            migrationBuilder.RenameTable(
                name: "MembersPositions",
                newName: "UserRoles");

            migrationBuilder.RenameTable(
                name: "Positions",
                newName: "Roles");

            migrationBuilder.RenameTable(
                name: "Members",
                newName: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_LeaderId",
                table: "UserRoles",
                column: "LeaderId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_SemesterId",
                table: "UserRoles",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_BigBroId",
                table: "Users",
                column: "BigBroId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ExpectedGraduationId",
                table: "Users",
                column: "ExpectedGraduationId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PledgeClassId",
                table: "Users",
                column: "PledgeClassId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_StatusId",
                table: "Users",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Addresses_dbo.Members_UserId",
                table: "Addresses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ClassesTaken_dbo.Members_UserId",
                table: "ClassesTaken",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.IncidentReports_dbo.Members_ReportedBy",
                table: "IncidentReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.LaundrySignups_dbo.Members_UserId",
                table: "LaundrySignups",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.MajorToMembers_dbo.Members_UserId",
                table: "MajorToMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.MealVotes_dbo.Members_UserId",
                table: "MealItemVotes",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.MealLatePlates_dbo.Members_UserId",
                table: "MealPlates",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.RoomToMembers_dbo.Members_UserId",
                table: "RoomToMembers",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceEventAmendments_dbo.Members_UserId",
                table: "ServiceEventAmendments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.Events_dbo.Members_SubmitterId",
                table: "ServiceEvents",
                column: "SubmitterId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceHourAmendments_dbo.Members_UserId",
                table: "ServiceHourAmendments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.ServiceHours_dbo.Members_UserId",
                table: "ServiceHours",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.SoberSignups_dbo.Members_UserId",
                table: "SoberSignups",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
