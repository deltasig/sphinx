namespace Dsp.Web.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Chore
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int TypeId { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [StringLength(100)]
        public string Url { get; set; }

        [ForeignKey("TypeId")]
        public virtual ChoreType Type { get; set; }
        [InverseProperty("Chore")]
        public ICollection<ChoreAssignment> Assignments { get; set; }
    }
}
