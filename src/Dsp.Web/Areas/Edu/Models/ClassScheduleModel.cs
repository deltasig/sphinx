namespace Dsp.Web.Areas.Edu.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class ClassScheduleModel
    {
        public string SelectedUserName { get; set; }
        public Member Member { get; set; }
        public ClassTaken ClassTaken { get; set; }

        public IEnumerable<Class> AllClasses { get; set; }
        public IEnumerable<SelectListItem> Semesters { get; set; }
        public IEnumerable<ClassTaken> ClassesTaken { get; set; }
    }
}