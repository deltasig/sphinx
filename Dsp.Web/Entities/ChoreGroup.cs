namespace Dsp.Web.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ChoreGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int TypeId { get; set; }

        public int? SemesterId { get; set; }
        
        [Required, StringLength(50)]
        public string Name { get; set; }
        
        public string AvatarPath { get; set; }

        [ForeignKey("TypeId")]
        public virtual ChoreGroupType Type { get; set; }
        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
        [InverseProperty("Group")]
        public virtual ICollection<ChoreGroupToMember> Members { get; set; }
        [InverseProperty("Group")]
        public virtual ICollection<ChoreAssignment> Assignments { get; set; }
    }
}
