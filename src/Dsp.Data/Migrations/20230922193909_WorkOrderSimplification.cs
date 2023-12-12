using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class WorkOrderSimplification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex("IX_WorkOrders_UserId", "WorkOrders", "UserId");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "WorkOrders",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedOn",
                table: "WorkOrders",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "WorkOrders",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.Sql(@"
            UPDATE W
            SET W.CreatedOn = (
                SELECT TOP 1 S.ChangedOn
                FROM WorkOrderStatusChanges S
                WHERE S.WorkOrderId = W.WorkOrderId
                ORDER BY S.ChangedOn
            )
            FROM WorkOrders as W");

            migrationBuilder.Sql(@"
            UPDATE W
            SET W.ClosedOn = S.ChangedOn
            FROM WorkOrders as W
            JOIN WorkOrderStatusChanges as S
                ON S.WorkOrderId = W.WorkOrderId AND S.WorkOrderStatusId = 5");

            migrationBuilder.DropTable(
                name: "WorkOrderComments");

            migrationBuilder.DropTable(
                name: "WorkOrderPriorityChanges");

            migrationBuilder.DropTable(
                name: "WorkOrderStatusChanges");

            migrationBuilder.DropTable(
                name: "WorkOrderPriorities");

            migrationBuilder.DropTable(
                name: "WorkOrderStatuses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("IX_WorkOrders_UserId", "WorkOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "ClosedOn",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "WorkOrders");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "WorkOrders",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "WorkOrderComments",
                columns: table => new
                {
                    WorkOrderCommentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    SubmittedOn = table.Column<DateTime>(type: "datetime", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.WorkOrderComments", x => x.WorkOrderCommentId);
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderComments_dbo.Members_UserId",
                        column: x => x.UserId,
                        principalTable: "Members",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderComments_dbo.WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPriorities",
                columns: table => new
                {
                    WorkOrderPriorityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.WorkOrderPriorities", x => x.WorkOrderPriorityId);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderStatuses",
                columns: table => new
                {
                    WorkOrderStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.WorkOrderStatuses", x => x.WorkOrderStatusId);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderPriorityChanges",
                columns: table => new
                {
                    WorkOrderPriorityChangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderPriorityId = table.Column<int>(type: "int", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.WorkOrderPriorityChanges", x => x.WorkOrderPriorityChangeId);
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderPriorityChanges_dbo.Members_UserId",
                        column: x => x.UserId,
                        principalTable: "Members",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderPriorityChanges_dbo.WorkOrderPriorities_WorkOrderPriorityId",
                        column: x => x.WorkOrderPriorityId,
                        principalTable: "WorkOrderPriorities",
                        principalColumn: "WorkOrderPriorityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderPriorityChanges_dbo.WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkOrderStatusChanges",
                columns: table => new
                {
                    WorkOrderStatusChangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    WorkOrderId = table.Column<int>(type: "int", nullable: false),
                    WorkOrderStatusId = table.Column<int>(type: "int", nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dbo.WorkOrderStatusChanges", x => x.WorkOrderStatusChangeId);
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderStatusChanges_dbo.Members_UserId",
                        column: x => x.UserId,
                        principalTable: "Members",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderStatusChanges_dbo.WorkOrderStatuses_WorkOrderStatusId",
                        column: x => x.WorkOrderStatusId,
                        principalTable: "WorkOrderStatuses",
                        principalColumn: "WorkOrderStatusId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_dbo.WorkOrderStatusChanges_dbo.WorkOrders_WorkOrderId",
                        column: x => x.WorkOrderId,
                        principalTable: "WorkOrders",
                        principalColumn: "WorkOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderComments_UserId",
                table: "WorkOrderComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderComments_WorkOrderId",
                table: "WorkOrderComments",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPriorityChanges_UserId",
                table: "WorkOrderPriorityChanges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPriorityChanges_WorkOrderId",
                table: "WorkOrderPriorityChanges",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderPriorityChanges_WorkOrderPriorityId",
                table: "WorkOrderPriorityChanges",
                column: "WorkOrderPriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStatusChanges_UserId",
                table: "WorkOrderStatusChanges",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStatusChanges_WorkOrderId",
                table: "WorkOrderStatusChanges",
                column: "WorkOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkOrderStatusChanges_WorkOrderStatusId",
                table: "WorkOrderStatusChanges",
                column: "WorkOrderStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_dbo.WorkOrders_dbo.Members_UserId",
                table: "WorkOrders",
                column: "UserId",
                principalTable: "Members",
                principalColumn: "UserId");
        }
    }
}
