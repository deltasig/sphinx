namespace Dsp.WebCore.Areas.Members.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EditMemberInfoModel
{
    public User User { get; set; }

    public SelectList PledgeClasses { get; set; }
    public SelectList Semesters { get; set; }
    public SelectList Members { get; set; }
}
