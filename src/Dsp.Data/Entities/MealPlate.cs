using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class MealPlate
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime PlateDateTime { get; set; }

    public DateTime SignedUpOn { get; set; }

    public string Type { get; set; }

    public virtual Member User { get; set; }
}
