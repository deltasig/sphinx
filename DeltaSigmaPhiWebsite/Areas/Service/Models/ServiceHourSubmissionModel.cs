namespace DeltaSigmaPhiWebsite.Areas.Service.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;

    public class ServiceHourSubmissionModel
    {
        [Required]
        [Display(Name = "Event")]
        [DataType(DataType.Text)]
        public int SelectedEventId { get; set; }
        public IEnumerable<SelectListItem> Events { get; set; }

        [Required]
        [Display(Name = "Hours")]
        [DataType(DataType.Duration)]
        public double HoursServed { get; set; }
        public DateTime SoberDriveTime { get; set; }
    }
}