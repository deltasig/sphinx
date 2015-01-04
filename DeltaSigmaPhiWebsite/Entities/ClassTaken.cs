namespace DeltaSigmaPhiWebsite.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("ClassesTaken")]
    public partial class ClassTaken
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int UserId { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ClassId { get; set; }

        [Key]
        [Column(Order = 2)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int SemesterId { get; set; }

        [Display(Name = "Instructor")]
        [DataType(DataType.Text)]
        [StringLength(40)]
        public string Instructor { get; set; }

        [StringLength(1)]
        public string MidtermGrade { get; set; }

        [StringLength(1)]
        public string FinalGrade { get; set; }

        public bool? Dropped { get; set; }

        public virtual Class Class { get; set; }

        public virtual Semester Semester { get; set; }

        public virtual Member Member { get; set; }
    }
}
