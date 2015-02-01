﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DeltaSigmaPhiWebsite.Entities
{
    public partial class MealToItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealToItemId { get; set; }

        [Column(Order = 0)]
        [Index("IX_MealToItem", 0, IsUnique = true)]
        public int MealId { get; set; }

        [Column(Order = 1)]
        [Index("IX_MealToItem", 1, IsUnique = true)]
        public int MealItemId { get; set; }

        [ForeignKey("MealId")]
        public virtual Meal Meal { get; set; }

        [ForeignKey("MealItemId")]
        public virtual MealItem MealItem { get; set; }
    }
}
