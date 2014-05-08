namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Semester
    {
        public Semester()
        {
            ChoreGroups = new HashSet<ChoreGroup>();
            ClassesTakens = new HashSet<ClassTaken>();
            Leaders = new HashSet<Leader>();
            Members = new HashSet<Member>();
            OrganizationsJoineds = new HashSet<OrganizationsJoined>();
            PledgeClasses = new HashSet<PledgeClass>();
        }

        public int SemesterId { get; set; }

        public int Year { get; set; }

        [Required]
        [StringLength(10)]
        public string Term { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateStart { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DateEnd { get; set; }

        public virtual ICollection<ChoreGroup> ChoreGroups { get; set; }

        public virtual ICollection<ClassTaken> ClassesTakens { get; set; }

        public virtual ICollection<Leader> Leaders { get; set; }

        public virtual ICollection<Member> Members { get; set; }

        public virtual ICollection<OrganizationsJoined> OrganizationsJoineds { get; set; }

        public virtual ICollection<PledgeClass> PledgeClasses { get; set; }
    }
}
