using Microsoft.AspNetCore.Identity;
using System;

namespace Dsp.Data.Entities;

public partial class User : IdentityUser<int>
{
    [PersonalData]
    public string FirstName { get; set; }

    [PersonalData]
    public string LastName { get; set; }

    public int? MemberId { get; set; }

    public DateTime? CreatedOn { get; set; }

    [PersonalData]
    public string EmergencyContact { get; set; }

    [PersonalData]
    public string EmergencyRelation { get; set; }

    [PersonalData]
    public string EmergencyPhoneNumber { get; set; }

    public virtual Member MemberInfo { get; set; }
}
