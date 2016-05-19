namespace Dsp.Web.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;

    public class SoberType
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SoberTypeId { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(3000),DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [InverseProperty("SoberType")]
        public virtual ICollection<SoberSignup> Signups { get; set; }
    }
}
