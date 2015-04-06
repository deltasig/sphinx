namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SoberType
    {
        [Key]
        public int SoberTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<SoberSignup> Signups { get; set; }
    }
}
