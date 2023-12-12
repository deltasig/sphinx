using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class SoberType
{
    public int SoberTypeId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public virtual ICollection<SoberSignup> Signups { get; set; } = new List<SoberSignup>();
}
