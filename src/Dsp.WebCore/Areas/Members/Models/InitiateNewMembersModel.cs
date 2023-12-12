namespace Dsp.WebCore.Areas.Members.Models;

using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class InitiateNewMembersModel
{
    public int[] SelectedMemberIds { get; set; }
    public IEnumerable<SelectListItem> NewMembers { get; set; }
}