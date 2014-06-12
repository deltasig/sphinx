namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System.ComponentModel.DataAnnotations;

    public partial class Address
    {
        public int AddressId { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Address1 { get; set; }

        [StringLength(100)]
        public string Address2 { get; set; }

        [Required]
        [StringLength(50)]
        public string City { get; set; }

        [Required]
        [StringLength(2)]
        public string State { get; set; }

        public int PostalCode { get; set; }

        [StringLength(50)]
        public string Country { get; set; }

        public virtual Member Member { get; set; }
    }
}
