namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity;

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