namespace Dsp.Web.Areas.Service.Models
{
    using System.Collections.Generic;
    using Dsp.Data.Entities;

    public class ServiceHourIndexMemberRowModel
    {
        public Member Member { get; set; }
        public double Hours { get; set; }
        public IEnumerable<ServiceHour> ServiceHours { get; set; }
    }
}