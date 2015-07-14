namespace Dsp.Entities
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Position : IdentityRole<int, Leader>
    {
        [StringLength(100)]
        public string Description { get; set; }

        [StringLength(50)]
        public string Inquiries { get; set; }

        [Required, DefaultValue(PositionType.Active)]
        public PositionType Type { get; set; }

        [Required, DefaultValue(false)]
        public bool IsExecutive { get; set; }

        [Required, DefaultValue(false)]
        public bool IsElected { get; set; }

        [Required, DefaultValue(1)]
        public int DisplayOrder { get; set; }

        [Required, DefaultValue(false)]
        public bool IsDisabled { get; set; }

        [Required, DefaultValue(true)]
        public bool IsPublic { get; set; }

        [DefaultValue(true)]
        public bool CanBeRemoved { get; set; }

        [InverseProperty("Position")]
        public virtual ICollection<Leader> Leaders { get; set; }

        public Position() { }
        public Position(string name) { Name = name; }

        public enum PositionType
        {
            NonMember,
            Active,
            Pledge,
            Alumni
        }
    }
}
