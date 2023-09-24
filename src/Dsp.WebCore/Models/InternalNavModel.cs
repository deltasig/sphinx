namespace Dsp.WebCore.Models;

public class InternalNavModel
{
    public bool ShowErrorLogLink { get; }
    public bool ShowAppointmentsLink { get; }
    public bool ShowPositionsLink { get; }
    public bool ShowRegistrationLink { get; }
    public bool ShowSemesterLink { get; }
    public bool ShowStatusesLink { get; }
    public bool ShowAdminLink { get; }
    public bool ShowSobersLink { get; }
    public bool ShowMealsLink { get; }
    public bool ShowLaundryLink { get; }
    public bool ShowTopLeftLinks { get; }
    public bool ShowUpdatesLink { get; }

    public InternalNavModel(IEnumerable<string> userRoles)
    {
        var isAdmin = true;//userRoles.Any(r => r == "Administrator");
        var isActive = isAdmin;
        var isReleased = false;
        var isSecretary = isAdmin;
        var isPresident = isAdmin;
        var isVpGrowth = isAdmin;
        var isAcademics = isAdmin;
        var isService = isAdmin;
        var isDor = isAdmin;
        var isNme = isAdmin;
        var isMember = isAdmin;
        var isAffiliate = false;

        if (!isAdmin)
        {
            foreach (var r in userRoles)
            {
                switch (r)
                {
                    case "Administrator": break;
                    case "Active":
                        isActive = true;
                        isMember = true;
                        break;
                    case "Released": isReleased = isActive ? false : true; break;
                    case "Secretary": isSecretary = true; break;
                    case "President": isPresident = true; break;
                    case "Vice President Growth": isVpGrowth = true; break;
                    case "Academics": isAcademics = true; break;
                    case "Service": isService = true; break;
                    case "Director of Recruitment": isDor = true; break;
                    case "New Member Education": isNme = true; break;
                    case "New": isMember = true; break;
                    case "Neophyte": isMember = true; break;
                    case "Alumnus": isMember = true; break;
                    case "Advisor": isMember = true; break;
                    case "Affiliate": isAffiliate = true; break;
                    default: break;
                }
            }
        }

        ShowErrorLogLink = isAdmin;
        ShowAppointmentsLink = isPresident;
        ShowPositionsLink = isPresident;
        ShowRegistrationLink = isSecretary || isDor || isNme || isVpGrowth;
        ShowSemesterLink = isPresident || isSecretary || isAcademics || isService || isDor || isVpGrowth;
        ShowStatusesLink = isAdmin;
        ShowAdminLink = ShowErrorLogLink || ShowAppointmentsLink || ShowPositionsLink ||
                        ShowRegistrationLink || ShowSemesterLink || ShowStatusesLink;

        ShowSobersLink = isMember;
        ShowMealsLink = isMember || isAffiliate;
        ShowLaundryLink = isMember;
        ShowTopLeftLinks = ShowSobersLink || ShowMealsLink || ShowLaundryLink;
        ShowUpdatesLink = isMember;
    }
}
