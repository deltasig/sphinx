using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dsp.Data.Entities
{
    public class BugReport
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required, StringLength(2500, ErrorMessage = "Max length is 2500 characters or ~500 words")]
        public string Description { get; set; }

        [Required, StringLength(100), Display(Name = "URL Where Problem Exists")]
        public string UrlWithProblem { get; set; }

        [StringLength(2500)]
        public string Response { get; set; }

        public bool IsFixed { get; set; }

        public DateTime ReportedOn { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

        [InverseProperty("Report")]
        public virtual ICollection<BugImage> Images { get; set; }
    }

    public class BugImage
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int BugReportId { get; set; }

        [Column(TypeName = "image")]
        public byte[] Image { get; set; }

        [ForeignKey("BugReportId")]
        public virtual BugReport Report { get; set; }
    }
}
