namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class ClassFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClassFileId { get; set; }

        public int ClassId { get; set; }

        public int UserId { get; set; }

        public DateTime UploadedOn { get; set; }

        public DateTime? LastAccessedOn { get; set; }

        public int DownloadCount { get; set; }

        [StringLength(200)]
        public string AwsCode { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }
        
        [ForeignKey("UserId")]
        public virtual Member Uploader { get; set; }
        
        public virtual ICollection<ClassFileVote> ClassFileVotes { get; set; }
    }
}