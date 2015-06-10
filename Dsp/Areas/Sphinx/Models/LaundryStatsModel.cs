namespace Dsp.Areas.Dsp.Models
{
    using System.Collections.Generic;

    public class LaundryStatsModel
    {
        public string Semester { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public int TotalSignups { get; set; }
        public decimal WeekAverage { get; set; }
        public decimal MonthAverage { get; set; }

        public List<string> WeekChartXLabels { get; set; }
        public List<decimal> WeekChartXValues { get; set; }

        public List<string> MonthChartXLabels { get; set; }
        public List<int> MonthChartXValues { get; set; }
    }

    public class LaundryUsageStatsModel
    {
        public string Semester { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public int TotalSignups { get; set; }
        public int TotalMembers { get; set; }

        public List<string> ChartXLabels { get; set; }
        public List<int> ChartXValues { get; set; }
    }
}