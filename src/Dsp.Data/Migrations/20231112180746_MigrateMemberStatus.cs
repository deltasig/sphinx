using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateMemberStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            UPDATE Members 
            SET StatusId = 4
            WHERE LastName = 'Swearengin';
            ");

            migrationBuilder.Sql(@"
            UPDATE M1
            SET BigBroId = NULL
            FROM Members AS M1
            INNER JOIN Members AS M2 ON M1.BigBroId = M2.Id
            WHERE M2.StatusId = 5 OR M2.StatusId = 7;

            UPDATE AspNetUsers
            SET MemberId = NULL
            FROM AspNetUsers AS U
            INNER JOIN Members AS M ON M.Id = U.MemberId
            WHERE M.StatusId = 5 OR M.StatusId = 7;

            DELETE FROM Members
            WHERE StatusId = 5 OR StatusId = 7;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
