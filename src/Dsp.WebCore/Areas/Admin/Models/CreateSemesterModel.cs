namespace Dsp.WebCore.Areas.Admin.Models;

using Dsp.Data.Entities;
using System.Collections.Generic;

public class CreateSemesterModel
{
    public IEnumerable<string> GreekAlphabet { get; set; }
    public Semester Semester { get; set; }
    public PledgeClass PledgeClass { get; set; }
}
