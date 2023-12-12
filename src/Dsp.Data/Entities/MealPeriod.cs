using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class MealPeriod
{
    public int Id { get; set; }

    public string Name { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public virtual ICollection<MealItemToPeriod> Items { get; set; } = new List<MealItemToPeriod>();
}
