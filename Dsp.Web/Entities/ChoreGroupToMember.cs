namespace Dsp.Web.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ChoreGroupToMember
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Column(Order = 0), Index("IX_ChoreGroupToMember", 0, IsUnique = true)]
        public int ChoreGroupId { get; set; }

        [Required, Column(Order = 1), Index("IX_ChoreGroupToMember", 1, IsUnique = true)]
        public int UserId { get; set; }
        
        [ForeignKey("ChoreGroupId")]
        public virtual ChoreGroup Group { get; set; }
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
    }
}
