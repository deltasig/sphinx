using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dsp.Web.Entities;
using Dsp.Web.Migrations;

namespace Dsp.Web.Areas.Scholarships.Models
{
    public class SubmitScholarshipAppModel
    {
        public ScholarshipApp App { get; set; }
        public ScholarshipSubmission Submission { get; set; }
        public IList<ScholarshipAnswer> Answers { get; set; } 
    }
}