using System;
using System.Collections.Generic;
using System.Linq;

namespace Dsp.Data.Entities;

public partial class PledgeClass
{
    public int PledgeClassId { get; set; }

    public int SemesterId { get; set; }

    public DateTime? PinningDate { get; set; }

    public DateTime? InitiationDate { get; set; }

    public string PledgeClassName { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual Semester Semester { get; set; }

    public string GetLetters()
    {
        var splits = PledgeClassName.Split(' ');
        var isTrueAlpha = splits.Contains("Alpha") && (splits.Contains("1") || splits.Contains("2") || splits.Contains("3"));
        if (isTrueAlpha) return "&Alpha;";

        return string.Join("", splits.Select(m => "&" + m + ";"));
    }
}
