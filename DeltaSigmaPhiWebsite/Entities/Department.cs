namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Department
    {
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string Name { get; set; }

        public virtual ICollection<Major> Majors { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
    }
}
