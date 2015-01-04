namespace DeltaSigmaPhiWebsite.Areas.Members.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class RosterIndexSearchModel
    {
        public string SearchTerm { get; set; }

        public int SelectedStatusId { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public int SelectedPledgeClassId { get; set; }
        public IEnumerable<SelectListItem> PledgeClasses { get; set; }
        public int SelectedGraduationSemesterId { get; set; }
        public IEnumerable<SelectListItem> Semesters { get; set; }

        public string LivingType { get; set; }

        public bool CustomSearchRequested()
        {
            return LivingType == "InHouse" ||
                LivingType == "OutOfHouse" ||
                SelectedStatusId != -1 || 
                SelectedPledgeClassId != -1 ||
                SelectedGraduationSemesterId != -1;
        }
    }
}