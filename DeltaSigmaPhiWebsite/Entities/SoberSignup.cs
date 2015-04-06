namespace DeltaSigmaPhiWebsite.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class SoberSignup
    {
        [Key]
        public int SignupId { get; set; }

        public int? UserId { get; set; }

        [Display(Name = "Type")]
        public int SoberTypeId { get; set; }

        [Display(Name = "Description")]
        [StringLength(100)]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Date of Shift")]
        [DataType(DataType.Date)]
        public DateTime DateOfShift { get; set; }

        public DateTime? DateTimeSignedUp { get; set; }

        [ForeignKey("SoberTypeId")]
        public virtual SoberType SoberType { get; set; }
        public virtual Member Member { get; set; }
    }
}
