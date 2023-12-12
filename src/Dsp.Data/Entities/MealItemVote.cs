using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class MealItemVote
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int MealItemId { get; set; }

    public bool IsUpvote { get; set; }

    public virtual MealItem MealItem { get; set; }

    public virtual User User { get; set; }
}
