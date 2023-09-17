using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ExtraPhoneNumber
{
    public int PhoneNumberId { get; set; }

    public int UserId { get; set; }

    public string Type { get; set; }

    [ProtectedPersonalData]
    public string PhoneNumber { get; set; }

    public virtual Member User { get; set; }
}
