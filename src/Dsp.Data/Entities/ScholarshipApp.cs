﻿namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipApp
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipAppId { get; set; }
        
        public int ScholarshipTypeId { get; set; }
        
        [Required, StringLength(100), Display(Name = "Title")]
        public string Title { get; set; }
        
        [Required, StringLength(3000), DataType(DataType.MultilineText)]
        [Display(Name = "Description")]
        public string AdditionalText { get; set; }
        
        [Required, DataType(DataType.Date), Display(Name = "Open Date")]
        public DateTime OpensOn { get; set; }
        
        [Required, DataType(DataType.Date), Display(Name = "Close Date")]
        public DateTime ClosesOn { get; set; }
        
        public bool IsPublic { get; set; }

        [ForeignKey("ScholarshipTypeId")]
        public virtual ScholarshipType Type { get; set; }
        [InverseProperty("Application")]
        public virtual ICollection<ScholarshipSubmission> Submissions { get; set; }
        [InverseProperty("Application")]
        public virtual ICollection<ScholarshipAppQuestion> Questions { get; set; }
    }
}