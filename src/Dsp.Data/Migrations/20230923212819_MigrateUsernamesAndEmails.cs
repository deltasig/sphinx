using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateUsernamesAndEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            UPDATE Members
            SET NormalizedEmail = UPPER(Email)");

            migrationBuilder.Sql(@"
            UPDATE Members
            SET NormalizedUserName = UPPER(UserName)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            UPDATE Members
            SET NormalizedEmail = NULL");

            migrationBuilder.Sql(@"
            UPDATE Members
            SET NormalizedUserName = NULL");
        }
    }
}
