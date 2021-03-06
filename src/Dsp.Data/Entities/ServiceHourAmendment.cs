namespace Dsp.Data.Entities
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class ServiceHourAmendment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, Display(Name = "Member")]
        public int UserId { get; set; }

        [Required, Display(Name = "Semester")]
        public int SemesterId { get; set; }

        [Required, Range(-50,50)]
        [Display(Name = "Amount of Hours")]
        public double AmountHours { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Reason for Amendment")]
        public string Reason { get; set; }
        
        [ForeignKey("UserId")]
        public virtual Member Member { get; set; }
        [ForeignKey("SemesterId")]
        public virtual Semester Semester { get; set; }
    }
}
