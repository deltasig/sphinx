namespace DeltaSigmaPhiWebsite.Entities
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class MealItemType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MealItemTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public ICollection<MealItem> MealItems { get; set; }
    }
}
