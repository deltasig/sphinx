namespace Dsp.Data.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipAppQuestion
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipAppQuestionId { get; set; }

        public int ScholarshipAppId { get; set; }

        public int ScholarshipQuestionId { get; set; }
        
        [Required]
        public int FormOrder { get; set; }
        
        [Required]
        public bool IsOptional { get; set; }

        [ForeignKey("ScholarshipAppId")]
        public virtual ScholarshipApp Application { get; set; }
        [ForeignKey("ScholarshipQuestionId")]
        public virtual ScholarshipQuestion Question { get; set; }
    }
}