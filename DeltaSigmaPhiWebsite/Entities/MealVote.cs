using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace DeltaSigmaPhiWebsite.Entities
{
    public partial class MealVote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealVoteId { get; set; }

        [Column(Order = 0)]
        [Index("IX_MealVote", 0, IsUnique = true)]
        public int UserId { get; set; }

        [Column(Order = 1)]
        [Index("IX_MealVote", 1, IsUnique = true)]
        public int MealItemId { get; set; }

        public bool IsUpvote { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }

        [ForeignKey("MealItemId")]
        public virtual MealItem MealItem { get; set; }
    }
}
