namespace Dsp.WebCore.Areas.Members.Models
{
    using Dsp.Data.Entities;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using System.Collections.Generic;

    public class RosterIndexModel
    {
        public int SelectedSemester { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<SelectListItem> Semesters { get; set; }
        public IEnumerable<Member> Members { get; set; }
    }
}