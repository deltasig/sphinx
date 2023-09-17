using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChores : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChoreAssignments");

            migrationBuilder.DropTable(
                name: "ChoreGroupToMembers");

            migrationBuilder.DropTable(
                name: "ChorePeriods");

            migrationBuilder.DropTable(
                name: "Chores");

            migrationBuilder.DropTable(
                name: "ChoreGroups");

            migrationBuilder.DropTable(
                name: "ChoreTypes");

            migrationBuilder.DropTable(
                name: "ChoreGroupTypes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChoreGroupTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ChoreGroupTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChorePeriods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BeginsOnCst = table.Column<DateTime>(type: "datetime", nullable: false),
                    EndsOnCst = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ChorePeriods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChoreTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ChoreTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChoreGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterId = table.Column<int>(type: "int", nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    AvatarPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ChoreGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreGroups_dbo.ChoreGroupTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ChoreGroupTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreGroups_dbo.Semesters_SemesterId",
                        column: x => x.SemesterId,
                        principalTable: "Semesters",
                        principalColumn: "SemesterId");
                });

            migrationBuilder.CreateTable(
                name: "Chores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Url = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.Chores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dbo.Chores_dbo.ChoreTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ChoreTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChoreGroupToMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChoreGroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ChoreGroupToMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreGroupToMembers_dbo.ChoreGroups_ChoreGroupId",
                        column: x => x.ChoreGroupId,
                        principalTable: "ChoreGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreGroupToMembers_dbo.Members_UserId",
                        column: x => x.UserId,
                        principalTable: "Members",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChoreAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChoreId = table.Column<int>(type: "int", nullable: false),
                    EnforcementChoreId = table.Column<int>(type: "int", nullable: true),
                    EnforcerId = table.Column<int>(type: "int", nullable: true),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    GroupSignerId = table.Column<int>(type: "int", nullable: true),
                    PeriodId = table.Column<int>(type: "int", nullable: false),
                    CancelledOnCst = table.Column<DateTime>(type: "datetime", nullable: false),
                    CreatedOnCst = table.Column<DateTime>(type: "datetime", nullable: false),
                    DueOnCst = table.Column<DateTime>(type: "datetime", nullable: false),
                    EnforcerFeedback = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnforcerVerificationTimeCst = table.Column<DateTime>(type: "datetime", nullable: false),
                    EnforcerVerified = table.Column<bool>(type: "bit", nullable: true),
                    GroupCompleted = table.Column<bool>(type: "bit", nullable: false),
                    GroupCompletionTimeCst = table.Column<DateTime>(type: "datetime", nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    OpensOnCst = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.ChoreAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreAssignments_dbo.ChoreAssignments_EnforcementChoreId",
                        column: x => x.EnforcementChoreId,
                        principalTable: "ChoreAssignments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_dbo.ChoreAssignments_dbo.ChoreGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "ChoreGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreAssignments_dbo.ChorePeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "ChorePeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreAssignments_dbo.Chores_ChoreId",
                        column: x => x.ChoreId,
                        principalTable: "Chores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.ChoreAssignments_dbo.Members_EnforcerId",
                        column: x => x.EnforcerId,
                        principalTable: "Members",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_dbo.ChoreAssignments_dbo.Members_GroupSignerId",
                        column: x => x.GroupSignerId,
                        principalTable: "Members",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChoreAssignments_ChoreId",
                table: "ChoreAssignments",
                column: "ChoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreAssignments_EnforcementChoreId",
                table: "ChoreAssignments",
                column: "EnforcementChoreId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreAssignments_EnforcerId",
                table: "ChoreAssignments",
                column: "EnforcerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreAssignments_GroupId",
                table: "ChoreAssignments",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreAssignments_GroupSignerId",
                table: "ChoreAssignments",
                column: "GroupSignerId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreAssignments_PeriodId",
                table: "ChoreAssignments",
                column: "PeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreGroups_SemesterId",
                table: "ChoreGroups",
                column: "SemesterId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreGroups_TypeId",
                table: "ChoreGroups",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreGroupToMembers_ChoreGroupId",
                table: "ChoreGroupToMembers",
                column: "ChoreGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoreGroupToMembers_UserId",
                table: "ChoreGroupToMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Chores_TypeId",
                table: "Chores",
                column: "TypeId");
        }
    }
}
