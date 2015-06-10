namespace Dsp.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Address
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AddressId { get; set; }

        public int UserId { get; set; }

        [Required, StringLength(20), Display(Name = "Type")]
        public string Type { get; set; }

        [StringLength(100), Display(Name = "Address 1")]
        public string Address1 { get; set; }

        [StringLength(100), Display(Name = "Address 2")]
        public string Address2 { get; set; }

        [StringLength(50)]
        public string City { get; set; }

        [StringLength(2)]
        public string State { get; set; }
        
        [Display(Name = "Postal Code")]
        public int PostalCode { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

        public bool IsFilledOut()
        {
            return !string.IsNullOrEmpty(Address1) && !string.IsNullOrEmpty(City) && !string.IsNullOrEmpty(State);
        }
    }
}
