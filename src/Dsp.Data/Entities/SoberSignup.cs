using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class SoberSignup
{
    public int SignupId { get; set; }

    public int? UserId { get; set; }

    public int SoberTypeId { get; set; }

    public string Description { get; set; }

    public DateTime DateOfShift { get; set; }

    public DateTime? DateTimeSignedUp { get; set; }

    public DateTime? CreatedOn { get; set; }

    public virtual SoberType SoberType { get; set; }

    public virtual User User { get; set; }
}
