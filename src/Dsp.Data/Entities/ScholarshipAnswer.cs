﻿namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipAnswer
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipAnswerId { get; set; }
        
        public Guid ScholarshipSubmissionId { get; set; }
        
        public int ScholarshipQuestionId { get; set; }
        
        [StringLength(3000), DataType(DataType.MultilineText), Display(Name = "Answer")]
        public string AnswerText { get; set; }

        [ForeignKey("ScholarshipSubmissionId")]
        public virtual ScholarshipSubmission Submission { get; set; }
        [ForeignKey("ScholarshipQuestionId")]
        public virtual ScholarshipQuestion Question { get; set; }
    }
}