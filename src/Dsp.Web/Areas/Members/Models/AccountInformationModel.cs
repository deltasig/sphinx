namespace Dsp.Web.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class AccountInformationModel
    {
        public Member Member { get; set; }
        public Semester CurrentSemester { get; set; }
        public IEnumerable<ClassTaken> ThisSemesterCourses { get; set; }
    }
}