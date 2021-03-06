namespace Dsp.Data.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ClassesTaken")]
    public class ClassTaken
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ClassTakenId { get; set; }

        [Required, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Index("IX_ClassTaken", 0, IsUnique = true)]
        public int UserId { get; set; }

        [Required, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Index("IX_ClassTaken", 1, IsUnique = true), Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required, Column(Order = 2), DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Index("IX_ClassTaken", 2, IsUnique = true), Display(Name = "Semesters")]
        public int SemesterId { get; set; }

        public bool IsSummerClass { get; set; }

        public DateTime? CreatedOn { get; set; }

        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }
        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
    }
}
