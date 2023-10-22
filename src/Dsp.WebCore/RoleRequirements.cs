using Microsoft.AspNetCore.Authorization;

namespace Dsp.WebCore;

public class MemberStatusRequirement : IAuthorizationRequirement
{
    public string[] Statuses { get; }

    public MemberStatusRequirement(params string[] statuses) => Statuses = statuses;
}

public class MemberAppointmentRequirement : IAuthorizationRequirement
{
    public string[] Positions { get; }

    public MemberAppointmentRequirement(params string[] positions) => Positions = positions;
}
