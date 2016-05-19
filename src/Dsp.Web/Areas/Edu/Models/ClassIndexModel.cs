namespace Dsp.Web.Areas.Edu.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class ClassIndexModel
    {
        public IEnumerable<Class> Classes { get; set; }
        public Semester CurrentSemester { get; set; }
    }
}