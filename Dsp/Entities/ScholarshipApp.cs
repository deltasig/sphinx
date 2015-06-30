﻿namespace Dsp.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;

    public class ScholarshipApp
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipAppId { get; set; }
        
        public int ScholarshipTypeId { get; set; }
        
        [Required, StringLength(100), Display(Name = "Title")]
        public string Title { get; set; }
        
        [Required, StringLength(3000), DataType(DataType.MultilineText), AllowHtml]
        [Display(Name = "Description"), UIHint("tinymce_full")]
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