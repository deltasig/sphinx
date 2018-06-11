namespace Dsp.Data.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class MealItemVote
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }

        public int MealItemId { get; set; }

        public bool IsUpvote { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
        [ForeignKey("MealItemId")]
        public virtual MealItem MealItem { get; set; }
    }
}
