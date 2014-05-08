namespace DeltaSigmaPhiWebsite.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class Class
    {
        public Class()
        {
            ClassesTakens = new HashSet<ClassTaken>();
            Instructors = new HashSet<Instructor>();
        }

        public int ClassId { get; set; }

        public int DepartmentId { get; set; }

        public int CourseNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; }

        public virtual ICollection<ClassTaken> ClassesTakens { get; set; }

        public virtual ICollection<Instructor> Instructors { get; set; }
    }
}
