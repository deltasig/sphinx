namespace Dsp.Areas.Edu.Models
{
    using Entities;
    using System.Web.Mvc;

    public class CreateClassModel
    {
        public SelectList Departments { get; set; }
        public Class Class { get; set; }
    }
}