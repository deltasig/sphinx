namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("MemberStatuses")]
    public class MemberStatus
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int StatusId { get; set; }

        [Required, StringLength(50)]
        public string StatusName { get; set; }

        [InverseProperty("MemberStatus")]
        public virtual ICollection<Member> Members { get; set; }
    }
}
