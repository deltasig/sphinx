namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Donation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int FundraiserId { get; set; }

        [Required, StringLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(50), Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Email { get; set; }

        [Column("PhoneNumber"), DataType(DataType.PhoneNumber), Display(Name = "Preferred Phone Number")]
        [RegularExpression(@"^(([0-9]{3}[-]([0-9]{3})[-][0-9]{4})|([+]?[0-9]{11,15}))$",
            ErrorMessage = "Phone number format was invalid.")]
        public string PhoneNumber { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be at least 1 cent.")]
        public double Amount { get; set; }

        public DateTime? ReceivedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        [ForeignKey("FundraiserId")]
        public virtual Fundraiser Fundraiser { get; set; }
    }
}
