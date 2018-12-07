namespace Dsp.Data.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Fundraiser
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CauseId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required, Range(0, double.MaxValue)]
        public double Goal { get; set; }

        [Required, Display(Name = "Begins")]
        public DateTime BeginsOn { get; set; }

        [Display(Name = "Ends")]
        public DateTime? EndsOn { get; set; }

        [Required, StringLength(2000), DataType(DataType.MultilineText), Display(Name = "Description")]
        public string Description { get; set; }

        [Required, StringLength(2000), DataType(DataType.MultilineText), Display(Name = "Donation Instructions")]
        public string DonationInstructions { get; set; }

        [Required, Display(Name = "Display on Donate Page")]
        public bool IsPublic { get; set; }

        [Required, Display(Name = "Allow New Members")]
        public bool IsPledgeable { get; set; }

        [ForeignKey("CauseId")]
        public virtual Cause Cause { get; set; }

        [InverseProperty("Fundraiser")]
        public virtual ICollection<Donation> Donations { get; set; }
    }
}
