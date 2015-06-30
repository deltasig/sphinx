﻿namespace Dsp.Areas.Kitchen.Models
{
    using Entities;
    using System;
    using System.Collections.Generic;

    public class MealScheduleModel
    {
        public DateTime StartOfWeek { get; set; }
        public IEnumerable<Meal> Meals { get; set; }
        public IEnumerable<MealPeriod> MealPeriods { get; set; }
        public IList<MealToPeriod> MealsToPeriods { get; set; }
        public IEnumerable<MealVote> UsersVotes { get; set; }
        public IEnumerable<MealLatePlate> LatePlates { get; set; }
    }
}