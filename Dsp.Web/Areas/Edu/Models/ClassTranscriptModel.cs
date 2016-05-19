namespace Dsp.Web.Areas.Edu.Models
{
    using Entities;
    using System.Collections.Generic;

    public class ClassTranscriptModel
    {
        public IEnumerable<object> Grades { get; set; }
        public ClassTaken ClassTaken { get; set; }
        public Member Member { get; set; }
    }
}