namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipAnswer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipAnswerId { get; set; }

        public Guid ScholarshipSubmissionId { get; set; }

        public int ScholarshipQuestionId { get; set; }

        [StringLength(3000)]
        [Display(Name = "Answer")]
        [DataType(DataType.MultilineText)]
        public string AnswerText { get; set; }

        [ForeignKey("ScholarshipSubmissionId")]
        public virtual ScholarshipSubmission Submission { get; set; }

        [ForeignKey("ScholarshipQuestionId")]
        public virtual ScholarshipQuestion Question { get; set; }
    }
}