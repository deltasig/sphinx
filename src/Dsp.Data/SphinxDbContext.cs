namespace Dsp.Data
{
    using Entities;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity;

    public class SphinxDbContext : IdentityDbContext
        <Member, Position, int, SphinxUserLogin, Leader, SphinxUserClaim>
    {
        public SphinxDbContext() : base("DefaultConnection")
        {
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Class> Classes { get; set; }
        public virtual DbSet<ClassFile> ClassFiles { get; set; }
        public virtual DbSet<ClassTaken> ClassesTaken { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Donation> Donations { get; set; }
        public virtual DbSet<Email> Emails { get; set; }
        public virtual DbSet<EmailType> EmailTypes { get; set; }
        public virtual DbSet<ElmahErrorLog> Errors { get; set; }
        public virtual DbSet<Fundraiser> Fundraisers { get; set; }
        public virtual DbSet<IncidentReport> IncidentReports { get; set; }
        public virtual DbSet<LaundrySignup> LaundrySignups { get; set; }
        public virtual DbSet<Leader> Leaders { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<MajorToMember> MajorsToMembers { get; set; }
        public virtual DbSet<Meal> Meals { get; set; }
        public virtual DbSet<MealItem> MealItems { get; set; }
        public virtual DbSet<MealPlate> MealPlates { get; set; }
        public virtual DbSet<MealPeriod> MealPeriods { get; set; }
        public virtual DbSet<MealToItem> MealToItems { get; set; }
        public virtual DbSet<MealToPeriod> MealToPeriods { get; set; }
        public virtual DbSet<MealItemVote> MealVotes { get; set; }
        public virtual DbSet<MemberStatus> MemberStatuses { get; set; }
        public virtual DbSet<Cause> Philanthropies { get; set; }
        public virtual DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public virtual DbSet<PledgeClass> PledgeClasses { get; set; }
        public virtual DbSet<Room> Rooms { get; set; }
        public virtual DbSet<RoomToMember> RoomsToMembers { get; set; }
        public virtual DbSet<ScholarshipAnswer> ScholarshipAnswers { get; set; }
        public virtual DbSet<ScholarshipApp> ScholarshipApps { get; set; }
        public virtual DbSet<ScholarshipType> ScholarshipTypes { get; set; }
        public virtual DbSet<ScholarshipQuestion> ScholarshipQuestions { get; set; }
        public virtual DbSet<ScholarshipSubmission> ScholarshipSubmissions { get; set; }
        public virtual DbSet<Semester> Semesters { get; set; }
        public virtual DbSet<ServiceHourAmendment> ServiceHourAmendments { get; set; }
        public virtual DbSet<ServiceEventAmendment> ServiceEventAmendments { get; set; }
        public virtual DbSet<ServiceEvent> ServiceEvents { get; set; }
        public virtual DbSet<ServiceHour> ServiceHours { get; set; }
        public virtual DbSet<SoberSignup> SoberSignups { get; set; }
        public virtual DbSet<SoberType> SoberTypes { get; set; }
        public virtual DbSet<WorkOrder> WorkOrders { get; set; }
        public virtual DbSet<WorkOrderComment> WorkOrderComments { get; set; }
        public virtual DbSet<WorkOrderPriority> WorkOrderPriorities { get; set; }
        public virtual DbSet<WorkOrderPriorityChange> WorkOrderPriorityChanges { get; set; }
        public virtual DbSet<WorkOrderStatus> WorkOrderStatuses { get; set; }
        public virtual DbSet<WorkOrderStatusChange> WorkOrderStatusChanges { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Member>().ToTable("Members", "dbo").Property(p => p.Id).HasColumnName("UserId");
            modelBuilder.Entity<Position>().ToTable("Positions", "dbo").Property(p => p.Id).HasColumnName("PositionId");
            modelBuilder.Entity<Leader>().ToTable("Leaders", "dbo").Property(p => p.RoleId).HasColumnName("LeaderId");
            modelBuilder.Entity<Leader>().HasKey(u => new { u.UserId, u.RoleId, u.SemesterId });
            modelBuilder.Entity<SphinxUserLogin>().ToTable("MemberLogins", "dbo");
            modelBuilder.Entity<SphinxUserClaim>().ToTable("MemberClaims", "dbo");
            modelBuilder.Entity<ElmahErrorLog>().ToTable("ELMAH_Error");

            modelBuilder
                .Entity<MealItemVote>()
                .HasIndex(c => new { c.UserId, c.MealItemId })
                .IsUnique()
                .HasName("IX_User_MealItem");
            modelBuilder
                .Entity<MealToItem>()
                .HasIndex(c => new { c.MealId, c.MealItemId })
                .IsUnique()
                .HasName("IX_Meal_MealItem");
            modelBuilder
                .Entity<MealToPeriod>()
                .HasIndex(c => new { c.MealPeriodId, c.MealId, c.Date })
                .IsUnique()
                .HasName("IX_MealPeriod_Meal_Date");
        }

        public static SphinxDbContext Create()
        {
            return new SphinxDbContext();
        }
    }
}