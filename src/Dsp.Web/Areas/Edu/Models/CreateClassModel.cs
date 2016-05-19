namespace Dsp.Web.Areas.Edu.Models
{
    using Dsp.Data.Entities;
    using System.Web.Mvc;

    public class CreateClassModel
    {
        public SelectList Departments { get; set; }
        public Class Class { get; set; }
    }
}