namespace Dsp.Services.Models;

using System;
using System.Collections.Generic;
using System.Linq;

public class ServiceHourStats
{
    public int UnadjustedMemberCount { get; private set; }
    public int AdjustedMemberCount { get; private set; }
    public float TargetPercentage { get; private set; }
    public IEnumerable<string> Months { get; private set; }
    public IEnumerable<int> TargetMemberCount { get; private set; }
    public IEnumerable<int> MoreThanZeroHours { get; private set; }
    public IEnumerable<int> FiveOrMoreHours { get; private set; }
    public IEnumerable<int> TenOrMoreHours { get; private set; }
    public IEnumerable<int> FifteenOrMoreHours { get; private set; }
    public DateTime CalculatedOn { get; }

    public ServiceHourStats(
        int unadjustedMemberCount,
        int adjustedMemberCount,
        IEnumerable<string> months,
        IEnumerable<int> moreThanZeroHours,
        IEnumerable<int> fiveOrMoreHours,
        IEnumerable<int> tenOrMoreHours,
        IEnumerable<int> fifteenOrMoreHours,
        DateTime calculatedOn,
        float targetPercentage = 0.8f)
    {
        UnadjustedMemberCount = unadjustedMemberCount;
        AdjustedMemberCount = adjustedMemberCount;
        TargetPercentage = targetPercentage;
        var target = (int)Math.Ceiling(adjustedMemberCount * targetPercentage);
        TargetMemberCount = Enumerable.Repeat(target, moreThanZeroHours.Count());
        Months = months;
        MoreThanZeroHours = moreThanZeroHours;
        FiveOrMoreHours = fiveOrMoreHours;
        TenOrMoreHours = tenOrMoreHours;
        FifteenOrMoreHours = fifteenOrMoreHours;
        CalculatedOn = calculatedOn;
    }
}
