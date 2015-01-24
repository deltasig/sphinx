using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DeltaSigmaPhiWebsite.Entities
{
    public partial class MealItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealItemId { get; set; }

        public int MealItemTypeId { get; set; }

        [StringLength(150)]
        public string Name { get; set; }

        public bool IsGlutenFree { get; set; }

        [ForeignKey("MealItemTypeId")]
        public virtual MealItemType MealItemType { get; set; }

        public virtual ICollection<MealToItem> MealToItems { get; set; }

        public virtual ICollection<MealVote> MealVotes { get; set; }

    }
}
