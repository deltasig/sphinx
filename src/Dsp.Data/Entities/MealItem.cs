using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class MealItem
{
    public int Id { get; set; }

    public string Name { get; set; }

    public bool IsGlutenFree { get; set; }

    public int Upvotes { get; set; }

    public int Downvotes { get; set; }

    public virtual ICollection<MealItemVote> Votes { get; set; } = new List<MealItemVote>();

    public virtual ICollection<MealItemToPeriod> Periods { get; set; } = new List<MealItemToPeriod>();
}
