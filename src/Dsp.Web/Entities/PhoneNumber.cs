namespace Dsp.Web.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PhoneNumber
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PhoneNumberId { get; set; }

        public int UserId { get; set; }

        [Column("PhoneNumber"), DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^(([0-9]{3}[-]([0-9]{3})[-][0-9]{4})|([+]?[0-9]{11,15}))$", 
            ErrorMessage = "Phone number format was invalid.")]
        public string Number { get; set; }

        [StringLength(100)]
        public string Type { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
