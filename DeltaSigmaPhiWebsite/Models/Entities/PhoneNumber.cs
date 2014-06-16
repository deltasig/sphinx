namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PhoneNumber
    {
        public int PhoneNumberId { get; set; }

        public int UserId { get; set; }

        [Column("PhoneNumber")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^([0-9]{3})[-]([0-9]{3})[-]([0-9]{4})$", 
            ErrorMessage = "Phone numbers must entered in the following format: ###-###-####")]
        public string PhoneNumber1 { get; set; }

        [StringLength(100)]
        public string Type { get; set; }

        public virtual Member Member { get; set; }
    }
}
