using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dsp.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateIdentityData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            INSERT INTO AspNetUsers ([MemberId]
                                    ,[FirstName]
                                    ,[LastName]
                                    ,[Email]
                                    ,[EmailConfirmed]
                                    ,[PasswordHash]
                                    ,[SecurityStamp]
                                    ,[PhoneNumber]
                                    ,[PhoneNumberConfirmed]
                                    ,[TwoFactorEnabled]
                                    ,[LockoutEnabled]
                                    ,[AccessFailedCount]
                                    ,[UserName]
                                    ,[CreatedOn]
                                    ,[ConcurrencyStamp]
                                    ,[LockoutEnd]
                                    ,[NormalizedEmail]
                                    ,[NormalizedUserName]
                                    ,[EmergencyContact]
                                    ,[EmergencyPhoneNumber]
                                    ,[EmergencyRelation])
            SELECT [Id]
                  ,[FirstName]
                  ,[LastName]
                  ,[Email]
                  ,[EmailConfirmed]
                  ,[PasswordHash]
                  ,[SecurityStamp]
                  ,[PhoneNumber]
                  ,[PhoneNumberConfirmed]
                  ,[TwoFactorEnabled]
                  ,[LockoutEnabled]
                  ,[AccessFailedCount]
                  ,[UserName]
                  ,[CreatedOn]
                  ,[ConcurrencyStamp]
                  ,[LockoutEnd]
                  ,[NormalizedEmail]
                  ,[NormalizedUserName]
                  ,[EmergencyContact]
                  ,[EmergencyPhoneNumber]
                  ,[EmergencyRelation]
              FROM [dspdb].[dbo].[Members]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DELETE FROM AspNetUsers");
        }
    }
}
