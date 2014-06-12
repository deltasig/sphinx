namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Department
    {
        public Department()
        {
            Majors = new HashSet<Major>();
            Classes = new HashSet<Class>();
        }

        public int DepartmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string DepartmentName { get; set; }

        public virtual ICollection<Major> Majors { get; set; }

        public virtual ICollection<Class> Classes { get; set; }
    }
}
