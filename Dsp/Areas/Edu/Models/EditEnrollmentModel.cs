namespace Dsp.Areas.Edu.Models
{
    using Entities;
    using System.Collections.Generic;

    public class EditEnrollmentModel
    {
        public ClassTaken Enrollment { get; set; }
        public IEnumerable<object> Grades { get; set; }
    }
}
