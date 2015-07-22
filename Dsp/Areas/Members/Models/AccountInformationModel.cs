namespace Dsp.Areas.Members.Models
{
    using Entities;
    using System.Collections.Generic;

    public class AccountInformationModel
    {
        public Member Member { get; set; }
        public Semester CurrentSemester { get; set; }
        public IEnumerable<ClassTaken> ThisSemesterCourses { get; set; }
    }
}