namespace Dsp.Web.Areas.Scholarships.Models
{
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Entities;

    public class CreateScholarshipAppModel
    {
        public ScholarshipApp Application { get; set; }
        public IEnumerable<SelectListItem> Types {get;set;}
        public IList<QuestionSelectionModel> Questions { get; set; }
    }
}