namespace Dsp.Web.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class SoberSignup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SignupId { get; set; }

        public int? UserId { get; set; }

        [Display(Name = "Type")]
        public int SoberTypeId { get; set; }

        [StringLength(100), Display(Name = "Description")]
        public string Description { get; set; }

        [Required, DataType(DataType.Date), Display(Name = "Date of Shift")]
        public DateTime DateOfShift { get; set; }

        public DateTime? DateTimeSignedUp { get; set; }

        public DateTime? CreatedOn { get; set; }

        [ForeignKey("SoberTypeId")]
        public virtual SoberType SoberType { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
