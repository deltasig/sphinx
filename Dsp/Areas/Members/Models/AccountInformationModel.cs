namespace Dsp.Areas.Members.Models
{
    using Entities;
    using System.Collections.Generic;

    public class AccountInformationModel
    {
        public Member MemberInfo { get; set; }
        public LocalPasswordModel ChangePasswordModel { get; set; }
        public IEnumerable<ClassTaken> ThisSemesterCourses { get; set; }
        public Semester CurrentSemester { get; set; }
    }
}