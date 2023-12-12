namespace Dsp.WebCore.Areas.House.Models;

using System;
using System.ComponentModel.DataAnnotations;
using Dsp.Data.Entities;

public class WorkOrderArchiveModel
{
    public WorkOrder WorkOrder { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Submitted On")]
    public DateTime SubmittedOn { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [Display(Name = "Closed On")]
    public DateTime ClosedOn { get; set; }
}