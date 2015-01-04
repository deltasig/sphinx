namespace DeltaSigmaPhiWebsite.Areas.Service.Models
{
    using Entities;

    public class ServiceHourIndexModel
    {
        public Member Member { get; set; }
        public double Hours { get; set; }
        public Semester Semester { get; set; }
    }
}