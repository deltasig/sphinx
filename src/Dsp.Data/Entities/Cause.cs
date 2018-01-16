namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Causes")]
    public class Cause
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required, StringLength(5000), DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [InverseProperty("Cause")]
        public virtual ICollection<Fundraiser> Fundraisers { get; set; }
    }
}
