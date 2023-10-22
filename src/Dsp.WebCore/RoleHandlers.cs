using Dsp.Services.Interfaces;
using Dsp.WebCore.Extensions;
using Microsoft.AspNetCore.Authorization;

namespace Dsp.WebCore;

public class OneOfManyMemberStatusHandler : AuthorizationHandler<MemberStatusRequirement>
{
    private readonly IMemberService memberService;
    private readonly IRoleService roleService;

    public OneOfManyMemberStatusHandler(IMemberService memberService, IRoleService roleService)
    {
        this.memberService = memberService;
        this.roleService = roleService;
    }

    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, MemberStatusRequirement requirement)
    {
        var userName = context.User.GetUserName();
        var user = await memberService.GetMemberByUserNameAsync(userName);
        var isAdmin = await roleService.UserIsAdminAsync(user.Id);

        var requiredStatuses = requirement.Statuses;
        var hasRole = isAdmin || requiredStatuses.Contains(user.Status.StatusName);

        if (hasRole)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"Current user does not have any of the following statuses: {string.Join(",", requiredStatuses)}.")
            );
        }
    }
}

public class OneOfManyMemberAppointmentHandler : AuthorizationHandler<MemberAppointmentRequirement>
{
    private readonly IMemberService memberService;
    private readonly IPositionService positionService;
    private readonly IRoleService roleService;

    public OneOfManyMemberAppointmentHandler(IMemberService memberService, IPositionService positionService, IRoleService roleService)
    {
        this.memberService = memberService;
        this.positionService = positionService;
        this.roleService = roleService;
    }

    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, MemberAppointmentRequirement requirement)
    {
        var userName = context.User.GetUserName();
        var user = await memberService.GetMemberByUserNameAsync(userName);
        var isAdmin = await roleService.UserIsAdminAsync(user.Id);
        var requiredPositions = requirement.Positions;
        var hasPosition = false;
        if (isAdmin)
        {
            hasPosition = true;
        }
        else
        {
            var currentPositionsForUser = await positionService.GetCurrentPositionsByUserAsync(userName);
            var currentUserPositions = currentPositionsForUser
                .Select(x => x.Name)
                .ToList();
            hasPosition = requirement.Positions.Intersect(currentUserPositions).Any();
        }

        if (hasPosition)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail(new AuthorizationFailureReason(
                this,
                $"Current user is not appointed to any of the following positions: {string.Join(",", requiredPositions)}.")
            );
        }
    }
}
