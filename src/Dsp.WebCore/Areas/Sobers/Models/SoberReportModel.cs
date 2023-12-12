namespace Dsp.WebCore.Areas.Sobers.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class SoberReportModel
{
    public List<User> Members { get; set; }
    public int? SelectedSemester { get; set; }
    public Semester Semester { get; set; }
    public IEnumerable<SelectListItem> SemesterList { get; set; }
}