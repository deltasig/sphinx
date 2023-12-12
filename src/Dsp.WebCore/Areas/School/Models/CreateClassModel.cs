namespace Dsp.WebCore.Areas.School.Models;

using Dsp.Data.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

public class CreateClassModel
{
    public SelectList Departments { get; set; }
    public Class Class { get; set; }
}