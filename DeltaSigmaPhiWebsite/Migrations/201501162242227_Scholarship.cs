namespace DeltaSigmaPhiWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Scholarship : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ScholarshipAnswers",
                c => new
                    {
                        ScholarshipAnswerId = c.Int(nullable: false, identity: true),
                        ScholarshipSubmissionId = c.Guid(nullable: false),
                        ScholarshipQuestionId = c.Int(nullable: false),
                        AnswerText = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipAnswerId)
                .ForeignKey("dbo.ScholarshipQuestions", t => t.ScholarshipQuestionId, cascadeDelete: true)
                .ForeignKey("dbo.ScholarshipSubmissions", t => t.ScholarshipSubmissionId, cascadeDelete: true)
                .Index(t => t.ScholarshipSubmissionId)
                .Index(t => t.ScholarshipQuestionId);
            
            CreateTable(
                "dbo.ScholarshipQuestions",
                c => new
                    {
                        ScholarshipQuestionId = c.Int(nullable: false, identity: true),
                        Prompt = c.String(nullable: false, maxLength: 500),
                        AnswerMinimumLength = c.Int(nullable: false),
                        AnswerMaximumLength = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipQuestionId);
            
            CreateTable(
                "dbo.ScholarshipAppQuestions",
                c => new
                    {
                        ScholarshipAppQuestionId = c.Int(nullable: false, identity: true),
                        ScholarshipAppId = c.Int(nullable: false),
                        ScholarshipQuestionId = c.Int(nullable: false),
                        FormOrder = c.Int(nullable: false),
                        IsOptional = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipAppQuestionId)
                .ForeignKey("dbo.ScholarshipApps", t => t.ScholarshipAppId, cascadeDelete: true)
                .ForeignKey("dbo.ScholarshipQuestions", t => t.ScholarshipQuestionId, cascadeDelete: true)
                .Index(t => t.ScholarshipAppId)
                .Index(t => t.ScholarshipQuestionId);
            
            CreateTable(
                "dbo.ScholarshipApps",
                c => new
                    {
                        ScholarshipAppId = c.Int(nullable: false, identity: true),
                        ScholarshipTypeId = c.Int(nullable: false),
                        Title = c.String(nullable: false, maxLength: 100),
                        AdditionalText = c.String(nullable: false, maxLength: 3000),
                        OpensOn = c.DateTime(nullable: false),
                        ClosesOn = c.DateTime(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipAppId)
                .ForeignKey("dbo.ScholarshipTypes", t => t.ScholarshipTypeId, cascadeDelete: true)
                .Index(t => t.ScholarshipTypeId);
            
            CreateTable(
                "dbo.ScholarshipSubmissions",
                c => new
                    {
                        ScholarshipSubmissionId = c.Guid(nullable: false, identity: true),
                        ScholarshipAppId = c.Int(nullable: false),
                        IsWinner = c.Boolean(nullable: false),
                        SubmittedOn = c.Boolean(nullable: false),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        LastName = c.String(nullable: false, maxLength: 50),
                        StudentNumber = c.String(nullable: false),
                        PhoneNumber = c.String(nullable: false),
                        Address1 = c.String(nullable: false, maxLength: 100),
                        Address2 = c.String(maxLength: 100),
                        City = c.String(nullable: false, maxLength: 50),
                        State = c.String(nullable: false, maxLength: 2),
                        PostalCode = c.Int(nullable: false),
                        Country = c.String(nullable: false, maxLength: 50),
                        HighSchool = c.String(nullable: false, maxLength: 100),
                        ActSatScore = c.Int(nullable: false),
                        Gpa = c.Double(nullable: false),
                        HearAboutScholarship = c.String(nullable: false, maxLength: 100),
                    })
                .PrimaryKey(t => t.ScholarshipSubmissionId)
                .ForeignKey("dbo.ScholarshipApps", t => t.ScholarshipAppId, cascadeDelete: true)
                .Index(t => t.ScholarshipAppId);
            
            CreateTable(
                "dbo.ScholarshipTypes",
                c => new
                    {
                        ScholarshipTypeId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.ScholarshipTypeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ScholarshipAppQuestions", "ScholarshipQuestionId", "dbo.ScholarshipQuestions");
            DropForeignKey("dbo.ScholarshipApps", "ScholarshipTypeId", "dbo.ScholarshipTypes");
            DropForeignKey("dbo.ScholarshipSubmissions", "ScholarshipAppId", "dbo.ScholarshipApps");
            DropForeignKey("dbo.ScholarshipAnswers", "ScholarshipSubmissionId", "dbo.ScholarshipSubmissions");
            DropForeignKey("dbo.ScholarshipAppQuestions", "ScholarshipAppId", "dbo.ScholarshipApps");
            DropForeignKey("dbo.ScholarshipAnswers", "ScholarshipQuestionId", "dbo.ScholarshipQuestions");
            DropIndex("dbo.ScholarshipSubmissions", new[] { "ScholarshipAppId" });
            DropIndex("dbo.ScholarshipApps", new[] { "ScholarshipTypeId" });
            DropIndex("dbo.ScholarshipAppQuestions", new[] { "ScholarshipQuestionId" });
            DropIndex("dbo.ScholarshipAppQuestions", new[] { "ScholarshipAppId" });
            DropIndex("dbo.ScholarshipAnswers", new[] { "ScholarshipQuestionId" });
            DropIndex("dbo.ScholarshipAnswers", new[] { "ScholarshipSubmissionId" });
            DropTable("dbo.ScholarshipTypes");
            DropTable("dbo.ScholarshipSubmissions");
            DropTable("dbo.ScholarshipApps");
            DropTable("dbo.ScholarshipAppQuestions");
            DropTable("dbo.ScholarshipQuestions");
            DropTable("dbo.ScholarshipAnswers");
        }
    }
}
