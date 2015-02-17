namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Data.Entity;

    public partial class DspDbContext : DbContext
    {
        public DspDbContext() : base("name=DefaultConnection")
        {
            //Database.Log = sql => Debug.Write(sql);
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Class> Classes { get; set; }
        public virtual DbSet<ClassFile> ClassFiles { get; set; }
        public virtual DbSet<ClassFileVote> ClassFileVotes { get; set; }
        public virtual DbSet<ClassTaken> ClassesTaken { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<IncidentReport> IncidentReports { get; set; }
        public virtual DbSet<LaundrySignup> LaundrySignups { get; set; }
        public virtual DbSet<Leader> Leaders { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<MajorToMember> MajorsToMembers { get; set; }
        public virtual DbSet<Meal> Meals { get; set; }
        public virtual DbSet<MealItem> MealItems { get; set; }
        public virtual DbSet<MealItemType> MealItemTypes { get; set; }
        public virtual DbSet<MealLatePlate> MealLatePlates { get; set; }
        public virtual DbSet<MealPeriod> MealPeriods { get; set; }
        public virtual DbSet<MealToItem> MealToItems { get; set; }
        public virtual DbSet<MealToPeriod> MealToPeriods { get; set; }
        public virtual DbSet<MealVote> MealVotes { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<MemberStatus> MemberStatus { get; set; }
        public virtual DbSet<OrganizationPosition> OrganizationPositions { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<OrganizationsJoined> OrganizationsJoined { get; set; }
        public virtual DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public virtual DbSet<PledgeClass> PledgeClasses { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<ScholarshipAnswer> ScholarshipAnswers { get; set; }
        public virtual DbSet<ScholarshipApp> ScholarshipApps { get; set; }
        public virtual DbSet<ScholarshipType> ScholarshipTypes { get; set; }
        public virtual DbSet<ScholarshipQuestion> ScholarshipQuestions { get; set; }
        public virtual DbSet<ScholarshipSubmission> ScholarshipSubmissions { get; set; }
        public virtual DbSet<Semester> Semesters { get; set; }
        public virtual DbSet<ServiceHour> ServiceHours { get; set; }
        public virtual DbSet<SoberSignup> SoberSchedule { get; set; }
        public virtual DbSet<StudyHour> StudyHours { get; set; }
        public virtual DbSet<StudyAssignment> StudyAssignments { get; set; }
        public virtual DbSet<StudyPeriod> StudyPeriods { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<webpages_Membership> webpages_Membership { get; set; }
        public virtual DbSet<webpages_OAuthMembership> webpages_OAuthMembership { get; set; }
        public virtual DbSet<webpages_Roles> webpages_Roles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClassFile>()
                .HasMany(e => e.ClassFileVotes)
                .WithRequired(v => v.ClassFile)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.ClassFileVotes)
                .WithRequired(v => v.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.ClassFileUploads)
                .WithRequired(f => f.Uploader)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Event>()
                .HasMany(e => e.ServiceHours)
                .WithRequired(e => e.Event)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<IncidentReport>()
                .Property(u => u.ReportedBy)
                .HasColumnName("ReportedBy");

            modelBuilder.Entity<Leader>()
                .HasMany(e => e.Members)
                .WithMany(e => e.Committees)
                .Map(m => m.ToTable("CommitteeMembers").MapLeftKey("LeaderId").MapRightKey("UserId"));

            modelBuilder.Entity<Position>()
                .HasMany(e => e.Leaders)
                .WithRequired(e => e.Position)
                .WillCascadeOnDelete(false);
            
            #region Class, Department, ClassTaken

            modelBuilder.Entity<Class>()
                .HasMany(e => e.ClassesTaken)
                .WithRequired(e => e.Class)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<ClassTaken>()
                .Property(e => e.MidtermGrade)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<ClassTaken>()
                .Property(e => e.FinalGrade)
                .IsFixedLength()
                .IsUnicode(false);

            modelBuilder.Entity<Department>()
                .HasMany(e => e.Majors)
                .WithRequired(e => e.Department)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Department>()
                .HasMany(e => e.Classes)
                .WithRequired(e => e.Department)
                .WillCascadeOnDelete(false);

            #endregion

            #region Organization

            modelBuilder.Entity<Organization>()
                .HasMany(e => e.OrganizationPositions)
                .WithRequired(e => e.Organization)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Organization>()
                .HasMany(e => e.OrganizationsJoineds)
                .WithRequired(e => e.Organization)
                .WillCascadeOnDelete(false);

            #endregion

            #region Member
            modelBuilder.Entity<Member>()
                .HasMany(e => e.Addresses)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.ClassesTaken)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.IncidentReports)
                .WithRequired(e => e.Member)
                .HasForeignKey(e => e.ReportedBy)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.LaundrySignups)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.Leaders)
                .WithRequired(e => e.Member)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.LittleBrothers)
                .WithOptional(e => e.BigBrother)
                .HasForeignKey(e => e.BigBroId);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.OrganizationsJoined)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.PhoneNumbers)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.ServiceHours)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);
            
            modelBuilder.Entity<Member>()
                .HasMany(e => e.StudyHourApprovals)
                .WithRequired(e => e.Approver)
                .HasForeignKey(e => e.ApproverId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasOptional(e => e.webpages_Membership)
                .WithRequired(e => e.Member);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.webpages_OAuthMembership)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            #endregion

            #region Semester
            modelBuilder.Entity<Semester>()
                .HasMany(e => e.ClassesTakens)
                .WithRequired(e => e.Semester)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Semester>()
                .HasMany(e => e.Leaders)
                .WithRequired(e => e.Semester)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Semester>()
                .HasMany(e => e.Members)
                .WithRequired(e => e.GraduationSemester)
                .HasForeignKey(e => e.ExpectedGraduationId);

            modelBuilder.Entity<Semester>()
                .HasMany(e => e.OrganizationsJoineds)
                .WithRequired(e => e.Semester)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Semester>()
                .HasMany(e => e.PledgeClasses)
                .WithRequired(e => e.Semester)
                .WillCascadeOnDelete(false);
            #endregion
        }
    }
}
