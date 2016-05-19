namespace Dsp.Web.Areas.Admin.Models
{
    using Dsp.Data.Entities;
    using System.ComponentModel.DataAnnotations;

    public class CreateSemesterModel
    {
        public Semester Semester { get; set; }
        [Required]
        [Display(Name = "Pledge Class")]
        [DataType(DataType.Text)]
        [StringLength(25)]
        public string PledgeClassName { get; set; }
    }
}