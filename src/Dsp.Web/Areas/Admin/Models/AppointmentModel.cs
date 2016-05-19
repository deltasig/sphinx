namespace Dsp.Web.Areas.Admin.Models
{
    using Entities;
    using System.Collections.Generic;

    public class AppointmentModel
    {
        public Leader Leader { get; set; }
        public IEnumerable<object> Users { get; set; }
        public IEnumerable<object> Alumni { get; set; }
    }
}