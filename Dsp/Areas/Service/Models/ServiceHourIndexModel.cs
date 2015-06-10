namespace Dsp.Areas.Service.Models
{
    using System.Collections.Generic;
    using Entities;

    public class ServiceHourIndexModel
    {
        public Member Member { get; set; }
        public double Hours { get; set; }
        public Semester Semester { get; set; }
        public IEnumerable<ServiceHour> ServiceHours { get; set; }
    }
}