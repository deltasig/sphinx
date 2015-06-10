namespace Dsp.Areas.Edu.Models
{
    using Entities;

    public class AssignMajorModel
    {
        public int UserId { get; set; }
        public int MajorId { get; set; }
        public DegreeLevel DegreeLevel { get; set; }
    }
}