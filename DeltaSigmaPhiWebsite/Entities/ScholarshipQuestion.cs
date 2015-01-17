namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipQuestion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipQuestionId { get; set; }

        [Required]
        [StringLength(500)]
        [DataType(DataType.MultilineText)]
        public string Prompt { get; set; }
        
        [Required]
        [Range(1, 3000)]
        public int AnswerMinimumLength { get; set; }

        [Required]
        [Range(1, 3000)]
        public int AnswerMaximumLength { get; set; }

        public virtual ICollection<ScholarshipAppQuestion> AppQuestions { get; set; }
        public virtual ICollection<ScholarshipAnswer> Answers { get; set; }
    }
}