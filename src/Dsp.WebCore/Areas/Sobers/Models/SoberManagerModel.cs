namespace Dsp.WebCore.Areas.Sobers.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

public class SoberManagerModel
{
    public IEnumerable<SoberSignup> Signups { get; set; }
    public SoberSignup NewSignup { get; set; }
    public MultiAddSoberSignupModel MultiAddModel { get; set; }
    public SelectList SignupTypes { get; set; }
}