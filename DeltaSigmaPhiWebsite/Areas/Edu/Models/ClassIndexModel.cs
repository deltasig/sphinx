namespace DeltaSigmaPhiWebsite.Areas.Edu.Models
{
    using Entities;
    using System.Collections.Generic;

    public class ClassIndexModel
    {
        public IEnumerable<Class> Classes { get; set; }
        public Semester CurrentSemester { get; set; }
    }
}