namespace Dsp.WebCore.Areas.Scholarships.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class CreateScholarshipAppModel
{
    public ScholarshipApp Application { get; set; }
    public IEnumerable<SelectListItem> Types {get;set;}
    public IList<QuestionSelectionModel> Questions { get; set; }
}