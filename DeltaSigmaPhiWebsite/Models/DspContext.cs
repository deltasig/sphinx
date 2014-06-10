namespace DeltaSigmaPhiWebsite.Models
{
    using System.Data.Entity;

    public partial class DspContext : DbContext
    {
        public DspContext()
            : base("name=DefaultConnection")
        {
        }

        public virtual DbSet<Address> Addresses { get; set; }
        public virtual DbSet<Class> Classes { get; set; }
        public virtual DbSet<ClassTaken> ClassesTakens { get; set; }
        public virtual DbSet<Department> Departments { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<IncidentReport> IncidentReports { get; set; }
        public virtual DbSet<Instructor> Instructors { get; set; }
        public virtual DbSet<LaundrySignup> LaundrySignups { get; set; }
        public virtual DbSet<Leader> Leaders { get; set; }
        public virtual DbSet<Major> Majors { get; set; }
        public virtual DbSet<Meal> Meals { get; set; }
        public virtual DbSet<MealCooked> MealsCooked { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<MemberStatus> MemberStatus { get; set; }
        public virtual DbSet<OrganizationPosition> OrganizationPositions { get; set; }
        public virtual DbSet<Organization> Organizations { get; set; }
        public virtual DbSet<OrganizationsJoined> OrganizationsJoined { get; set; }
        public virtual DbSet<PhoneNumber> PhoneNumbers { get; set; }
        public virtual DbSet<PledgeClass> PledgeClasses { get; set; }
        public virtual DbSet<Position> Positions { get; set; }
        public virtual DbSet<Semester> Semesters { get; set; }
        public virtual DbSet<ServiceHour> ServiceHours { get; set; }
        public virtual DbSet<SoberDriver> SoberDrivers { get; set; }
        public virtual DbSet<SoberOfficer> SoberOfficers { get; set; }
        public virtual DbSet<StudyHour> StudyHours { get; set; }
        public virtual DbSet<sysdiagram> sysdiagrams { get; set; }
        public virtual DbSet<webpages_Membership> webpages_Membership { get; set; }
        public virtual DbSet<webpages_OAuthMembership> webpages_OAuthMembership { get; set; }
        public virtual DbSet<webpages_Roles> webpages_Roles { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>()
                .HasMany(e => e.ServiceHours)
                .WithRequired(e => e.Event)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<IncidentReport>()
                .Property(u => u.ReportedBy)
                .HasColumnName("ReportedBy");

            modelBuilder.Entity<Leader>()
                .HasMany(e => e.Members)
                .WithMany(e => e.Leaders1)
                .Map(m => m.ToTable("CommitteeMembers").MapLeftKey("LeaderId").MapRightKey("UserId"));

            modelBuilder.Entity<Meal>()
                .HasMany(e => e.MealsCooked)
                .WithRequired(e => e.Meal)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Position>()
                .HasMany(e => e.Leaders)
                .WithRequired(e => e.Position)
                .WillCascadeOnDelete(false);
            
            #region Class, Department, ClassTaken

            modelBuilder.Entity<Class>()
                .HasMany(e => e.ClassesTaken)
                .WithRequired(e => e.Class)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Class>()
                .HasMany(e => e.Instructors)
                .WithMany(e => e.Classes)
                .Map(m => m.ToTable("ClassesTaught").MapLeftKey("ClassId").MapRightKey("InstructorId"));

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

            modelBuilder.Entity<Major>()
                .HasMany(e => e.Members)
                .WithMany(e => e.Majors)
                .Map(m => m.ToTable("MemberMajors").MapLeftKey("MajorId").MapRightKey("UserId"));

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
                .HasMany(e => e.StudyHours)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasOptional(e => e.webpages_Membership)
                .WithRequired(e => e.Member);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.webpages_OAuthMembership)
                .WithRequired(e => e.Member)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Member>()
                .HasMany(e => e.webpages_Roles)
                .WithMany(e => e.Members)
                .Map(m => m.ToTable("webpages_UsersInRoles").MapLeftKey("UserId").MapRightKey("RoleId"));

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
                .WithOptional(e => e.Semester)
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
