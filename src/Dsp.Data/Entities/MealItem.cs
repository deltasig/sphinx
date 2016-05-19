namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MealItem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealItemId { get; set; }

        [Required, Display(Name = "Type")]
        public int MealItemTypeId { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required]
        public bool IsGlutenFree { get; set; }

        [ForeignKey("MealItemTypeId")]
        public virtual MealItemType MealItemType { get; set; }
        [InverseProperty("MealItem")]
        public virtual ICollection<MealToItem> MealToItems { get; set; }
        [InverseProperty("MealItem")]
        public virtual ICollection<MealVote> MealVotes { get; set; }

        public string GetGlutenLabel()
        {
            return IsGlutenFree ? "Yes" : "No";
        }
    }
}
