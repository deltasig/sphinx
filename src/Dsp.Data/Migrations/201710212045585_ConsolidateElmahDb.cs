namespace Dsp.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConsolidateElmahDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ELMAH_Error",
                c => new
                    {
                        ErrorId = c.Guid(nullable: false),
                        Application = c.String(maxLength: 60),
                        Host = c.String(maxLength: 50),
                        Type = c.String(maxLength: 100),
                        Source = c.String(maxLength: 60),
                        Message = c.String(maxLength: 500),
                        User = c.String(maxLength: 50),
                        StatusCode = c.Int(nullable: false),
                        TimeUtc = c.DateTime(nullable: false),
                        Sequence = c.Int(),
                        AllXml = c.String(),
                    })
                .PrimaryKey(t => t.ErrorId);

            Sql("EXEC('ALTER TABLE [dbo].[ELMAH_Error] ADD CONSTRAINT[DF_ELMAH_Error_ErrorId] DEFAULT(NEWID()) FOR[ErrorId]')");

            Sql(@"EXEC('CREATE NONCLUSTERED INDEX [IX_ELMAH_Error_App_Time_Seq] ON [dbo].[ELMAH_Error] 
                (
                    [Application]   ASC,
                    [TimeUtc]       DESC,
                    [Sequence]      DESC
                )')");

            Sql(@"EXEC('CREATE PROCEDURE [dbo].[ELMAH_GetErrorXml] (@Application NVARCHAR(60), @ErrorId UNIQUEIDENTIFIER) AS
                    SET NOCOUNT ON
                    SELECT [AllXml] FROM [ELMAH_Error] WHERE [ErrorId] = @ErrorId AND [Application] = @Application')");

            Sql(@"EXEC('CREATE PROCEDURE [dbo].[ELMAH_GetErrorsXml]
                (@Application NVARCHAR(60), @PageIndex INT = 0, @PageSize INT = 15, @TotalCount INT OUTPUT)
                AS  
                    SET NOCOUNT ON 
                    DECLARE @FirstTimeUTC DATETIME
                    DECLARE @FirstSequence INT
                    DECLARE @StartRow INT
                    DECLARE @StartRowIndex INT
  
                    SELECT @TotalCount = COUNT(1) FROM [ELMAH_Error] WHERE [Application] = @Application
  
                    SET @StartRowIndex = @PageIndex * @PageSize + 1
  
                    IF @StartRowIndex <= @TotalCount
                    BEGIN 
                        SET ROWCOUNT @StartRowIndex
  
                        SELECT @FirstTimeUTC = [TimeUtc], @FirstSequence = [Sequence] FROM [ELMAH_Error]
                        WHERE [Application] = @Application ORDER BY [TimeUtc] DESC, [Sequence] DESC 
                    END
                    ELSE
                    BEGIN 
                        SET @PageSize = 0 
                    END
  
                    SET ROWCOUNT @PageSize
  
                    SELECT 
                        errorId     = [ErrorId], 
                        application = [Application],
                        host        = [Host], 
                        type        = [Type],
                        source      = [Source],
                        message     = [Message],
                        [user]      = [User],
                        statusCode  = [StatusCode], 
                        time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + ''Z''
                    FROM [ELMAH_Error] error WHERE [Application] = @Application AND [TimeUtc] <= @FirstTimeUTC
                    AND [Sequence] <= @FirstSequence ORDER BY [TimeUtc] DESC, [Sequence] DESC FOR XML AUTO')");

            Sql(@"EXEC('CREATE PROCEDURE [dbo].[ELMAH_LogError] (@ErrorId UNIQUEIDENTIFIER, @Application NVARCHAR(60), @Host NVARCHAR(30),
                  @Type NVARCHAR(100), @Source NVARCHAR(60), @Message NVARCHAR(500), @User NVARCHAR(50), @AllXml NTEXT, @StatusCode INT,
                  @TimeUtc DATETIME) AS 
                   
                 SET NOCOUNT ON
         
                 INSERT INTO [ELMAH_Error] ([ErrorId], [Application], [Host], [Type], [Source], [Message], [User], [AllXml], [StatusCode], [TimeUtc])
                 VALUES (@ErrorId, @Application, @Host, @Type, @Source, @Message, @User, @AllXml, @StatusCode, @TimeUtc)')");
        }
        
        public override void Down()
        {
            Sql("EXEC('DROP PROCEDURE [ELMAH_GetErrorXml]')");
            Sql("EXEC('DROP PROCEDURE [ELMAH_GetErrorsXml]')");
            Sql("EXEC('DROP PROCEDURE [ELMAH_LogError]')");
            DropTable("dbo.ELMAH_Error");
        }
    }
}
