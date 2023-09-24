namespace Dsp.WebCore.Areas.Sobers.Models;

using System.ComponentModel.DataAnnotations;

public class MultiAddSoberSignupModel
{
    [Range(0, 5)]
    public int DriverAmount { get; set; }
    [Range(0, 5)]
    public int OfficerAmount { get; set; }
    public string DateString { get; set; }
}
