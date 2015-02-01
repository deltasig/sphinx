namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;


    public partial class MealItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealItemId { get; set; }

        [Required]
        public int MealItemTypeId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [Required]
        public bool IsGlutenFree { get; set; }

        [ForeignKey("MealItemTypeId")]
        public virtual MealItemType MealItemType { get; set; }

        public virtual ICollection<MealToItem> MealToItems { get; set; }

        public virtual ICollection<MealVote> MealVotes { get; set; }

        public string GetGlutenLabel()
        {
            return IsGlutenFree ? "Yes" : "No";
        }

    }
}
