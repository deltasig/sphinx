namespace Dsp.Web.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Department
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DepartmentId { get; set; }

        [Required, StringLength(100), Display(Name = "Name")]
        public string Name { get; set; }

        [InverseProperty("Department")]
        public virtual ICollection<Major> Majors { get; set; }
        [InverseProperty("Department")]
        public virtual ICollection<Class> Classes { get; set; }
    }
}
