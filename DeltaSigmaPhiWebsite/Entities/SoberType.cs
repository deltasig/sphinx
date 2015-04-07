namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Web.Mvc;

    public partial class SoberType
    {
        [Key]
        public int SoberTypeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(2000)]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [UIHint("tinymce_full")]
        public string Description { get; set; }

        public virtual ICollection<SoberSignup> Signups { get; set; }
    }
}
