namespace Dsp.Data
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;

    public class ElmahDbContext : DbContext
    {
        public ElmahDbContext() : base("Elmah")
        {
        }

        public virtual DbSet<ElmahErrorLog> Errors { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ElmahErrorLog>().ToTable("ELMAH_Error");
        }
    }

    public class ElmahErrorLog
    {
        [Key]
        public Guid ErrorId { get; set; }
        [StringLength(60)]
        public string Application { get; set; }
        [StringLength(50)]
        public string Host { get; set; }
        [StringLength(100)]
        public string Type { get; set; }
        [StringLength(60)]
        public string Source { get; set; }
        [StringLength(500)]
        public string Message { get; set; }
        [StringLength(50)]
        public string User { get; set; }
        public int StatusCode { get; set; }
        public DateTime TimeUtc { get; set; }
        public int? Sequence { get; set; }
        public string AllXml { get; set; }
    }
}