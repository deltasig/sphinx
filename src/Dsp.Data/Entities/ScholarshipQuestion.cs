namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipQuestion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipQuestionId { get; set; }

        [Required, StringLength(500), DataType(DataType.MultilineText)]
        public string Prompt { get; set; }

        [Required, Range(1, 3000), Display(Name = "Minimum Length (characters)")]
        public int AnswerMinimumLength { get; set; }

        [Required, Range(1, 3000), Display(Name = "Maximum Length (characters)")]
        public int AnswerMaximumLength { get; set; }

        [InverseProperty("Question")]
        public virtual ICollection<ScholarshipAppQuestion> AppQuestions { get; set; }
        [InverseProperty("Question")]
        public virtual ICollection<ScholarshipAnswer> Answers { get; set; }
    }
}