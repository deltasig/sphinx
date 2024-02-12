namespace Dsp.WebCore.Areas.School.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class ClassScheduleModel
{
    public string SelectedUserName { get; set; }
    public Member User { get; set; }
    public ClassTaken ClassTaken { get; set; }

    public IEnumerable<Class> AllClasses { get; set; }
    public IEnumerable<SelectListItem> Semesters { get; set; }
    public IEnumerable<ClassTaken> ClassesTaken { get; set; }
}