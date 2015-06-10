namespace Dsp.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class EmailType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EmailTypeId { get; set; }

        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(100)]
        public string Destination { get; set; }

        [InverseProperty("EmailType")]
        public virtual ICollection<Email> Emails { get; set; }
    }
}