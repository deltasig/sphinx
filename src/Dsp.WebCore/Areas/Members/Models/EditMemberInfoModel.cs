namespace Dsp.WebCore.Areas.Members.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class EditMemberInfoModel
{
    public User User { get; set; }

    public IEnumerable<SelectListItem> Statuses { get; set; }
    public IEnumerable<SelectListItem> PledgeClasses { get; set; }
    public IEnumerable<SelectListItem> Semesters { get; set; }
    public IEnumerable<SelectListItem> Members { get; set; }
    public IEnumerable<SelectListItem> ShirtSizes { get; set; }
}
