namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class webpages_Roles
    {
        public webpages_Roles()
        {
            Members = new HashSet<Member>();
        }

        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(256)]
        public string RoleName { get; set; }

        public virtual ICollection<Member> Members { get; set; }
    }
}
