namespace DeltaSigmaPhiWebsite.Models.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Class
    {
        public Class()
        {
            ClassesTaken = new HashSet<ClassTaken>();
            Instructors = new HashSet<Instructor>();
        }

        public int ClassId { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Number")]
        [DataType(DataType.Text)]
        [StringLength(7)]
        public string CourseShorthand { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        public string CourseName { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }

        public virtual ICollection<Instructor> Instructors { get; set; }
    }
}
