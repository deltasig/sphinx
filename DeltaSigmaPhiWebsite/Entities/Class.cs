namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class Class
    {
        public Class()
        {
            ClassesTaken = new HashSet<ClassTaken>();
        }

        public int ClassId { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Number")]
        [DataType(DataType.Text)]
        [StringLength(15)]
        [MinLength(1)]
        public string CourseShorthand { get; set; }

        [Required]
        [Display(Name = "Name")]
        [StringLength(100)]
        [MinLength(1)]
        public string CourseName { get; set; }

        [Required]
        [Display(Name = "Credit Hours")]
        [Range(1, 6, ErrorMessage = "Please enter a valid number of credit hours: 1-6.")]
        public int CreditHours { get; set; }

        public virtual Department Department { get; set; }

        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }
    }
}
