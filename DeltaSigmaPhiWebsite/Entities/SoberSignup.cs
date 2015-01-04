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

        [Required]
        public SoberSignupType Type { get; set; }

        [Required]
        [Display(Name = "Date of Shift")]
        [DataType(DataType.Date)]
        public DateTime DateOfShift { get; set; }

        public DateTime? DateTimeSignedUp { get; set; }

        public virtual Member Member { get; set; }
    }

    public enum SoberSignupType
    {
        Driver,
        Officer
    }    
}
