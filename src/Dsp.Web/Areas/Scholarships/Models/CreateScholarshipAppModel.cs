namespace Dsp.Web.Areas.Scholarships.Models
{
    using Dsp.Data.Entities;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class CreateScholarshipAppModel
    {
        public ScholarshipApp Application { get; set; }
        public IEnumerable<SelectListItem> Types {get;set;}
        public IList<QuestionSelectionModel> Questions { get; set; }
    }
}