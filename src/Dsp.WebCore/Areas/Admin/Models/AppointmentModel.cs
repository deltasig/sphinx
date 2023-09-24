namespace Dsp.WebCore.Areas.Admin.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class AppointmentModel
{
    public Semester Semester { get; set; }
    public IEnumerable<SelectListItem> SemesterList { get; set; }
    public IEnumerable<Role> Positions { get; set; }
}
