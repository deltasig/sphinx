namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public partial class Position
    {
        public Position()
        {
            Leaders = new HashSet<Leader>();
        }

        public int PositionId { get; set; }

        [Required]
        [StringLength(50)]
        public string PositionName { get; set; }

        [StringLength(100)]
        public string Description { get; set; }

        [Required]
        [DefaultValue(PositionType.Active)]
        public PositionType Type { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsExecutive { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsElected { get; set; }

        [Required]
        [DefaultValue(1)]
        public int DisplayOrder { get; set; }

        [Required]
        [DefaultValue(false)]
        public bool IsDisabled { get; set; }

        [Required]
        [DefaultValue(true)]
        public bool IsPublic { get; set; }

        [DefaultValue(true)]
        public bool CanBeRemoved { get; set; }

        public virtual ICollection<Leader> Leaders { get; set; }
        
        public enum PositionType
        {
            Active,
            Pledge,
            Alumni
        }
    }
}
