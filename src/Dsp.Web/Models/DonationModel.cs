using Dsp.Data.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Dsp.Web.Models
{
    public class DonationPledgeModel
    {
        public IEnumerable<Fundraiser> ActiveFundraisers { get; set; }
        public SelectList PledgeableFundraisers { get; set; }

        [Required, Display(Name = "Fundraiser")]
        public int FundraiserId { get; set; }

        [Required, StringLength(100), Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required, StringLength(50), Display(Name = "Last Name")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }

        [DataType(DataType.PhoneNumber), Display(Name = "Preferred Phone Number (###-###-####)")]
        [RegularExpression(@"^(([0-9]{3}[-]([0-9]{3})[-][0-9]{4})|([+]?[0-9]{11,15}))$",
            ErrorMessage = "Phone number format was invalid. Please include dashes as follows: ###-###-####")]
        public string PhoneNumber { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "The amount must be at least 1 cent.")]
        public double Amount { get; set; }
    }
}