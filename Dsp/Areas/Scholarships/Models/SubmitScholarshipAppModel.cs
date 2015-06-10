using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dsp.Entities;
using Dsp.Migrations;

namespace Dsp.Areas.Scholarships.Models
{
    public class SubmitScholarshipAppModel
    {
        public ScholarshipApp App { get; set; }
        public ScholarshipSubmission Submission { get; set; }
        public IList<ScholarshipAnswer> Answers { get; set; } 
    }
}