namespace DeltaSigmaPhiWebsite.Areas.Study.Models
{
    using Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class StudyPeriodInfoModel
    {
        public StudyPeriod Period { get; set; }
        public StudyAssignment Assignment { get; set; }
        public int[] SelectedMemberIds { get; set; }
        public IEnumerable<SelectListItem> Members { get; set; }
    }
}