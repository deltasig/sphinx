namespace Dsp.Entities
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Leader : IdentityUserRole<int>
    {
        [Key, Required]
        public int SemesterId { get; set; }

        public DateTime AppointedOn { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

        [ForeignKey("RoleId")]
        public virtual Position Position { get; set; }

        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
    }
}
