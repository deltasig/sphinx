using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dsp.Web.Areas.Sphinx.Models
{
    public class Notification
    {
        public string Message { get; set; }
        public string LinkText { get; set; }
        public string Link { get; set; }
        public string Why { get; set; }
        public NotificationPriority Priority { get; set; }
    }

    public enum NotificationPriority
    {
        Low,
        Moderate,
        High
    }
}
