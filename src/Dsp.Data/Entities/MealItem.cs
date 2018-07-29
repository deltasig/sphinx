namespace Dsp.Data.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MealItem
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; }

        [Required]
        public bool IsGlutenFree { get; set; }

        public int Upvotes { get; set; }

        public int Downvotes { get; set; }

        [InverseProperty("MealItem")]
        public virtual ICollection<MealItemToPeriod> MealPeriods { get; set; }
        [InverseProperty("MealItem")]
        public virtual ICollection<MealItemVote> MealVotes { get; set; }

        public string GetGlutenLabel()
        {
            return IsGlutenFree ? "Yes" : "No";
        }
    }
}
