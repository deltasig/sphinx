namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class PhoneNumber
    {
        public int PhoneNumberId { get; set; }

        public int UserId { get; set; }

        [Column("PhoneNumber")]
        public int PhoneNumber1 { get; set; }

        public virtual Member Member { get; set; }
    }
}
