namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ScholarshipType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ScholarshipTypeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }
        
        [InverseProperty("Type")]
        public virtual ICollection<ScholarshipApp> Applications { get; set; }
    }
}