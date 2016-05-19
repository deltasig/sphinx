namespace Dsp.Web.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Major
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MajorId { get; set; }

        [Required, Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required, Display(Name = "Name"), StringLength(100)]
        public string MajorName { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        [InverseProperty("Major")]
        public virtual ICollection<MajorToMember> MajorToMembers { get; set; }
    }
}
