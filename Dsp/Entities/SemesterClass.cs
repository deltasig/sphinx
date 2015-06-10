namespace Dsp.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("SemesterClasses")]
    public class SemesterClass
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SemesterClassId { get; set; }

        [Required, Column(Order = 0), DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Index("IX_SemesterClass", 0, IsUnique = true), Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required, Column(Order = 1), DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Index("IX_SemesterClass", 1, IsUnique = true), Display(Name = "Semester")]
        public int SemesterId { get; set; }

        [ForeignKey("ClassId")]
        public virtual Class Class { get; set; }
        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
    }
}
