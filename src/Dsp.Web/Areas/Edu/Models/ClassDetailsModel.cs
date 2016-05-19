namespace Dsp.Web.Areas.Edu.Models
{
    using Entities;

    public class ClassDetailsModel
    {
        public Class Class { get; set; }
        public Semester CurrentSemester { get; set; }
        public FileInfoModel FileInfoModel { get; set; }
    }
}