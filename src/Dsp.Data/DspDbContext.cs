using Dsp.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dsp.Data;

public partial class DspDbContext :
        IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public DspDbContext()
    {
    }

    public DspDbContext(DbContextOptions<DspDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Address> Addresses { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<ClassTaken> ClassesTaken { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<IncidentReport> IncidentReports { get; set; }

    public virtual DbSet<LaundrySignup> LaundrySignups { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<MajorToMember> MajorsToMembers { get; set; }

    public virtual DbSet<MealItem> MealItems { get; set; }

    public virtual DbSet<MealItemVote> MealItemVotes { get; set; }

    public virtual DbSet<MealItemToPeriod> MealItemsToPeriods { get; set; }

    public virtual DbSet<MealPeriod> MealPeriods { get; set; }

    public virtual DbSet<MealPlate> MealPlates { get; set; }

    public virtual DbSet<PledgeClass> PledgeClasses { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomToMember> RoomsToMembers { get; set; }

    public virtual DbSet<ScholarshipAnswer> ScholarshipAnswers { get; set; }

    public virtual DbSet<ScholarshipApp> ScholarshipApps { get; set; }

    public virtual DbSet<ScholarshipAppQuestion> ScholarshipAppQuestions { get; set; }

    public virtual DbSet<ScholarshipQuestion> ScholarshipQuestions { get; set; }

    public virtual DbSet<ScholarshipSubmission> ScholarshipSubmissions { get; set; }

    public virtual DbSet<ScholarshipType> ScholarshipTypes { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<ServiceEvent> ServiceEvents { get; set; }

    public virtual DbSet<ServiceEventAmendment> ServiceEventAmendments { get; set; }

    public virtual DbSet<ServiceHour> ServiceHours { get; set; }

    public virtual DbSet<ServiceHourAmendment> ServiceHourAmendments { get; set; }

    public virtual DbSet<SoberSignup> SoberSignups { get; set; }

    public virtual DbSet<SoberType> SoberTypes { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<WorkOrder> WorkOrders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("PK_dbo.MemberStatuses");

            entity.Property(e => e.StatusName)
                .IsRequired()
                .HasMaxLength(50);

            entity.ToTable("UserTypes");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.Positions");
            entity.Property(e => e.Id).HasColumnName("PositionId");

            entity.Property(e => e.Description).HasMaxLength(100);
            entity.Property(e => e.Inquiries).HasMaxLength(50);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(256);

            entity.ToTable("Roles");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.Members");
            entity.Property(e => e.Id).HasColumnName("UserId");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DietaryInstructions).HasMaxLength(50);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.LastUpdatedOn).HasColumnType("datetime");
            entity.Property(e => e.UserName)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.EmergencyContact)
                .IsRequired(false)
                .HasMaxLength(50);
            entity.Property(e => e.EmergencyRelation)
                .IsRequired(false)
                .HasMaxLength(50);
            entity.Property(e => e.EmergencyPhoneNumber)
                .IsRequired(false);

            entity.HasOne(d => d.BigBro).WithMany(p => p.LittleBros)
                .HasForeignKey(d => d.BigBroId)
                .HasConstraintName("FK_dbo.Members_dbo.Members_BigBroId");

            entity.HasOne(d => d.ExpectedGraduation).WithMany(p => p.GraduatingMembers)
                .HasForeignKey(d => d.ExpectedGraduationId)
                .HasConstraintName("FK_dbo.Members_dbo.Semesters_ExpectedGraduationId");

            entity.HasOne(d => d.PledgeClass).WithMany(p => p.Users)
                .HasForeignKey(d => d.PledgeClassId)
                .HasConstraintName("FK_dbo.Members_dbo.PledgeClasses_PledgeClassId");

            entity.HasOne(d => d.Status).WithMany(p => p.Users)
                .HasForeignKey(d => d.StatusId)
                .HasConstraintName("FK_dbo.Members_dbo.MemberStatuses_StatusId");

            entity.ToTable("Users");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId, e.SemesterId }).HasName("PK_dbo.Leaders");

            entity.Property(e => e.RoleId).HasColumnName("LeaderId");

            entity.Property(e => e.AppointedOn).HasColumnType("datetime");

            entity.HasOne(d => d.Semester).WithMany(p => p.Leaders)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_dbo.Leaders_dbo.Semesters_SemesterId");

            entity.HasOne(d => d.User).WithMany(p => p.Roles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.Leaders_dbo.Members_UserId");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK_dbo.Leaders_dbo.Members_RoleId");

            entity.ToTable("UserRoles");
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.AddressId).HasName("PK_dbo.Addresses");

            entity.Property(e => e.Address1).HasMaxLength(100);
            entity.Property(e => e.Address2).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(50);
            entity.Property(e => e.Country).HasMaxLength(50);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(20);

            entity.HasOne(d => d.User).WithMany(p => p.Addresses)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.Addresses_dbo.Members_UserId");

            entity.ToTable("Addresses");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK_dbo.Classes");

            entity.Property(e => e.CourseName)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.CourseShorthand)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Department).WithMany(p => p.Classes)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_dbo.Classes_dbo.Departments_DepartmentId");

            entity.ToTable("Classes");
        });

        modelBuilder.Entity<ClassTaken>(entity =>
        {
            entity.HasKey(e => e.ClassTakenId).HasName("PK_dbo.ClassesTaken");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");

            entity.HasOne(d => d.Class).WithMany(p => p.ClassesTaken)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK_dbo.ClassesTaken_dbo.Classes_ClassId");

            entity.HasOne(d => d.Semester).WithMany(p => p.ClassesTaken)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_dbo.ClassesTaken_dbo.Semesters_SemesterId");

            entity.HasOne(d => d.User).WithMany(p => p.ClassesTaken)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.ClassesTaken_dbo.Members_UserId");

            entity.ToTable("ClassesTaken");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartmentId).HasName("PK_dbo.Departments");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.ToTable("Departments");
        });

        modelBuilder.Entity<IncidentReport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.IncidentReports");

            entity.Property(e => e.DateTimeOfIncident).HasColumnType("datetime");
            entity.Property(e => e.DateTimeSubmitted).HasColumnType("datetime");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(1500);
            entity.Property(e => e.InvestigationNotes)
                .IsRequired()
                .HasMaxLength(3000)
                .HasDefaultValueSql("('')");
            entity.Property(e => e.OfficialReport).HasMaxLength(1500);
            entity.Property(e => e.PolicyBroken)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.IncidentReports)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.IncidentReports_dbo.Members_ReportedBy");

            entity.ToTable("IncidentReports");
        });

        modelBuilder.Entity<LaundrySignup>(entity =>
        {
            entity.HasKey(e => e.DateTimeShift).HasName("PK_dbo.LaundrySignups");

            entity.Property(e => e.DateTimeShift).HasColumnType("datetime");
            entity.Property(e => e.DateTimeSignedUp).HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.LaundrySignups)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.LaundrySignups_dbo.Members_UserId");

            entity.ToTable("LaundrySignups");
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasKey(e => e.MajorId).HasName("PK_dbo.Majors");

            entity.Property(e => e.MajorName)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Majors)
                .HasForeignKey(d => d.DepartmentId)
                .HasConstraintName("FK_dbo.Majors_dbo.Departments_DepartmentId");

            entity.ToTable("Majors");
        });

        modelBuilder.Entity<MajorToMember>(entity =>
        {
            entity.HasKey(e => e.MajorToMemberId).HasName("PK_dbo.MajorToMembers");

            entity.HasOne(d => d.Major).WithMany(p => p.MajorToMembers)
                .HasForeignKey(d => d.MajorId)
                .HasConstraintName("FK_dbo.MajorToMembers_dbo.Majors_MajorId");

            entity.HasOne(d => d.User).WithMany(p => p.Majors)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.MajorToMembers_dbo.Members_UserId");

            entity.ToTable("MajorToMembers");
        });

        modelBuilder.Entity<MealItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.MealItems");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(150);

            entity.ToTable("MealItems");
        });

        modelBuilder.Entity<MealItemVote>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.MealItemVotes");

            entity.HasIndex(e => new { e.UserId, e.MealItemId }, "IX_User_MealItem").IsUnique();

            entity.HasOne(d => d.MealItem).WithMany(p => p.Votes)
                .HasForeignKey(d => d.MealItemId)
                .HasConstraintName("FK_dbo.MealVotes_dbo.MealItems_MealItemId");

            entity.HasOne(d => d.User).WithMany(p => p.MealItemVotes)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.MealVotes_dbo.Members_UserId");

            entity.ToTable("MealItemVotes");
        });

        modelBuilder.Entity<MealItemToPeriod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.MealItemsToPeriods");

            entity.HasIndex(e => new { e.MealPeriodId, e.MealItemId, e.Date }, "IX_MealPeriod_MealItem_Date").IsUnique();

            entity.Property(e => e.Date).HasColumnType("date");

            entity.HasOne(d => d.MealItem).WithMany(p => p.Periods)
                .HasForeignKey(d => d.MealItemId)
                .HasConstraintName("FK_dbo.MealItemsToPeriods_dbo.MealItems_MealItemId");

            entity.HasOne(d => d.MealPeriod).WithMany(p => p.Items)
                .HasForeignKey(d => d.MealPeriodId)
                .HasConstraintName("FK_dbo.MealItemsToPeriods_dbo.MealPeriods_MealPeriodId");

            entity.ToTable("MealItemsToPeriods");
        });

        modelBuilder.Entity<MealPeriod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.MealPeriods");

            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.StartTime).HasColumnType("datetime");

            entity.ToTable("MealPeriods");
        });

        modelBuilder.Entity<MealPlate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.MealLatePlates");

            entity.Property(e => e.PlateDateTime).HasColumnType("datetime");
            entity.Property(e => e.SignedUpOn).HasColumnType("datetime");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(25);

            entity.HasOne(d => d.User).WithMany(p => p.MealPlates)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.MealLatePlates_dbo.Members_UserId");

            entity.ToTable("MealPlates");
        });

        modelBuilder.Entity<PledgeClass>(entity =>
        {
            entity.HasKey(e => e.PledgeClassId).HasName("PK_dbo.PledgeClasses");

            entity.Property(e => e.InitiationDate).HasColumnType("datetime");
            entity.Property(e => e.PinningDate).HasColumnType("datetime");
            entity.Property(e => e.PledgeClassName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Semester).WithMany(p => p.PledgeClasses)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_dbo.PledgeClasses_dbo.Semesters_SemesterId");

            entity.ToTable("PledgeClasses");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK_dbo.Rooms");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Semester).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_dbo.Rooms_dbo.Semesters_SemesterId");

            entity.ToTable("Rooms");
        });

        modelBuilder.Entity<RoomToMember>(entity =>
        {
            entity.HasKey(e => e.RoomToMemberId).HasName("PK_dbo.RoomToMembers");

            entity.Property(e => e.MovedIn).HasColumnType("datetime");
            entity.Property(e => e.MovedOut).HasColumnType("datetime");

            entity.HasOne(d => d.Room).WithMany(p => p.Members)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK_dbo.RoomToMembers_dbo.Rooms_RoomId");

            entity.HasOne(d => d.User).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.RoomToMembers_dbo.Members_UserId");

            entity.ToTable("RoomToMembers");
        });

        modelBuilder.Entity<ScholarshipAnswer>(entity =>
        {
            entity.HasKey(e => e.ScholarshipAnswerId).HasName("PK_dbo.ScholarshipAnswers");

            entity.Property(e => e.AnswerText).HasMaxLength(3000);

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ScholarshipQuestionId)
                .HasConstraintName("FK_dbo.ScholarshipAnswers_dbo.ScholarshipQuestions_ScholarshipQuestionId");

            entity.HasOne(d => d.Submission).WithMany(p => p.Answers)
                .HasForeignKey(d => d.ScholarshipSubmissionId)
                .HasConstraintName("FK_dbo.ScholarshipAnswers_dbo.ScholarshipSubmissions_ScholarshipSubmissionId");

            entity.ToTable("ScholarshipAnswers");
        });

        modelBuilder.Entity<ScholarshipApp>(entity =>
        {
            entity.HasKey(e => e.ScholarshipAppId).HasName("PK_dbo.ScholarshipApps");

            entity.Property(e => e.AdditionalText)
                .IsRequired()
                .HasMaxLength(3000);
            entity.Property(e => e.ClosesOn).HasColumnType("datetime");
            entity.Property(e => e.OpensOn).HasColumnType("datetime");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Type).WithMany(p => p.Applications)
                .HasForeignKey(d => d.ScholarshipTypeId)
                .HasConstraintName("FK_dbo.ScholarshipApps_dbo.ScholarshipTypes_ScholarshipTypeId");

            entity.ToTable("ScholarshipApps");
        });

        modelBuilder.Entity<ScholarshipAppQuestion>(entity =>
        {
            entity.HasKey(e => e.ScholarshipAppQuestionId).HasName("PK_dbo.ScholarshipAppQuestions");

            entity.HasOne(d => d.Application).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ScholarshipAppId)
                .HasConstraintName("FK_dbo.ScholarshipAppQuestions_dbo.ScholarshipApps_ScholarshipAppId");

            entity.HasOne(d => d.Question).WithMany(p => p.Questions)
                .HasForeignKey(d => d.ScholarshipQuestionId)
                .HasConstraintName("FK_dbo.ScholarshipAppQuestions_dbo.ScholarshipQuestions_ScholarshipQuestionId");

            entity.ToTable("ScholarshipAppQuestions");
        });

        modelBuilder.Entity<ScholarshipQuestion>(entity =>
        {
            entity.HasKey(e => e.ScholarshipQuestionId).HasName("PK_dbo.ScholarshipQuestions");

            entity.Property(e => e.Prompt)
                .IsRequired()
                .HasMaxLength(500);

            entity.ToTable("ScholarshipQuestions");
        });

        modelBuilder.Entity<ScholarshipSubmission>(entity =>
        {
            entity.HasKey(e => e.ScholarshipSubmissionId).HasName("PK_dbo.ScholarshipSubmissions");

            entity.Property(e => e.ScholarshipSubmissionId).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Address1)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.Address2).HasMaxLength(100);
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.CommitteeRespondedOn).HasColumnType("datetime");
            entity.Property(e => e.Country)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.HearAboutScholarship)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.HighSchool)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(50);
            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(15);
            entity.Property(e => e.State).HasMaxLength(2);
            entity.Property(e => e.StudentNumber)
                .IsRequired()
                .HasMaxLength(15);
            entity.Property(e => e.SubmittedOn).HasColumnType("datetime");

            entity.HasOne(d => d.Application).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.ScholarshipAppId)
                .HasConstraintName("FK_dbo.ScholarshipSubmissions_dbo.ScholarshipApps_ScholarshipAppId");

            entity.ToTable("ScholarshipSubmissions");
        });

        modelBuilder.Entity<ScholarshipType>(entity =>
        {
            entity.HasKey(e => e.ScholarshipTypeId).HasName("PK_dbo.ScholarshipTypes");

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.ToTable("ScholarshipTypes");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.Semesters");
            entity.Property(e => e.Id).HasColumnName("SemesterId");

            entity.Property(e => e.DateEnd).HasColumnType("datetime");
            entity.Property(e => e.DateStart).HasColumnType("datetime");
            entity.Property(e => e.TransitionDate).HasColumnType("datetime");

            entity.ToTable("Semesters");
        });

        modelBuilder.Entity<ServiceEvent>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK_dbo.ServiceEvents");

            entity.HasIndex(e => e.SemesterId, "IX_SemesterId");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DateTimeOccurred).HasColumnType("datetime");
            entity.Property(e => e.EventName)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasOne(d => d.Semester).WithMany(p => p.ServiceEvents)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_dbo.ServiceEvents_dbo.Semesters_SemesterId");

            entity.HasOne(d => d.Submitter).WithMany(p => p.ServiceEvents)
                .HasForeignKey(d => d.SubmitterId)
                .HasConstraintName("FK_dbo.Events_dbo.Members_SubmitterId");

            entity.ToTable("ServiceEvents");
        });

        modelBuilder.Entity<ServiceEventAmendment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.ServiceEventAmendments");

            entity.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Semester).WithMany(p => p.ServiceEventAmendments)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_dbo.ServiceEventAmendments_dbo.Semesters_SemesterId");

            entity.HasOne(d => d.User).WithMany(p => p.ServiceEventAmendments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.ServiceEventAmendments_dbo.Members_UserId");

            entity.ToTable("ServiceEventAmendments");
        });

        modelBuilder.Entity<ServiceHour>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.EventId }).HasName("PK_dbo.ServiceHours");

            entity.Property(e => e.DateTimeSubmitted).HasColumnType("datetime");

            entity.HasOne(d => d.Event).WithMany(p => p.ServiceHours)
                .HasForeignKey(d => d.EventId)
                .HasConstraintName("FK_dbo.ServiceHours_dbo.Events_EventId");

            entity.HasOne(d => d.User).WithMany(p => p.ServiceHours)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.ServiceHours_dbo.Members_UserId");

            entity.ToTable("ServiceHours");
        });

        modelBuilder.Entity<ServiceHourAmendment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_dbo.ServiceHourAmendments");

            entity.Property(e => e.Reason)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(d => d.Semester).WithMany(p => p.ServiceHourAmendments)
                .HasForeignKey(d => d.SemesterId)
                .HasConstraintName("FK_dbo.ServiceHourAmendments_dbo.Semesters_SemesterId");

            entity.HasOne(d => d.User).WithMany(p => p.ServiceHourAmendments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.ServiceHourAmendments_dbo.Members_UserId");

            entity.ToTable("ServiceHourAmendments");
        });

        modelBuilder.Entity<SoberSignup>(entity =>
        {
            entity.HasKey(e => e.SignupId).HasName("PK_dbo.SoberSignups");

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");
            entity.Property(e => e.DateOfShift).HasColumnType("datetime");
            entity.Property(e => e.DateTimeSignedUp).HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(100);

            entity.HasOne(d => d.SoberType).WithMany(p => p.Signups)
                .HasForeignKey(d => d.SoberTypeId)
                .HasConstraintName("FK_dbo.SoberSignups_dbo.SoberTypes_SoberTypeId");

            entity.HasOne(d => d.User).WithMany(p => p.SoberSignups)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.SoberSignups_dbo.Members_UserId");

            entity.ToTable("SoberSignups");
        });

        modelBuilder.Entity<SoberType>(entity =>
        {
            entity.HasKey(e => e.SoberTypeId).HasName("PK_dbo.SoberTypes");

            entity.Property(e => e.Description).HasMaxLength(3000);
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.ToTable("SoberTypes");
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.HasKey(e => e.WorkOrderId).HasName("PK_dbo.WorkOrders");

            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(3000);
            entity.Property(e => e.Result).HasMaxLength(3000);
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedOn)
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(e => e.ClosedOn)
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.WorkOrders)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_dbo.WorkOrders_dbo.Members_UserId");

            entity.ToTable("WorkOrders");
        });
    }
}
