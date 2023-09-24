namespace Dsp.WebCore.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;

    public class AccountInformationModel
    {
        public User User { get; set; }
        public Semester CurrentSemester { get; set; }
        public IEnumerable<ClassTaken> ThisSemesterCourses { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}