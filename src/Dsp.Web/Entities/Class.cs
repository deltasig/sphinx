namespace Dsp.Web.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Class
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClassId { get; set; }

        [Required, Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required, MinLength(1), StringLength(50), DataType(DataType.Text), Display(Name = "Number")]
        public string CourseShorthand { get; set; }

        [Required, MinLength(1), StringLength(100), Display(Name = "Name")]
        public string CourseName { get; set; }

        [Required, Display(Name = "Credit Hours")]
        [Range(1, 6, ErrorMessage = "Please enter a valid number of credit hours: 1-6.")]
        public int CreditHours { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        [InverseProperty("Class")]
        public virtual ICollection<ClassTaken> ClassesTaken { get; set; }
        [InverseProperty("Class")]
        public virtual ICollection<ClassFile> ClassFiles { get; set; }
    }
}
