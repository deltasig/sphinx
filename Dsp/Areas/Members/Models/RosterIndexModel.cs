namespace Dsp.Areas.Members.Models
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class RosterIndexModel
    {
        public int SelectedSemester { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> Semesters { get; set; }
        public IEnumerable<Member> Members { get; set; }
    }
}