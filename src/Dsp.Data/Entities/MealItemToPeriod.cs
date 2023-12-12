using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class MealItemToPeriod
{
    public int Id { get; set; }

    public int MealPeriodId { get; set; }

    public int MealItemId { get; set; }

    public DateTime Date { get; set; }

    public virtual MealItem MealItem { get; set; }

    public virtual MealPeriod MealPeriod { get; set; }
}
