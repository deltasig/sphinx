using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DeltaSigmaPhiWebsite.Entities;
using DeltaSigmaPhiWebsite.Migrations;

namespace DeltaSigmaPhiWebsite.Areas.Scholarships.Models
{
    public class SubmitScholarshipAppModel
    {
        public ScholarshipApp App { get; set; }
        public ScholarshipSubmission Submission { get; set; }
        public IList<ScholarshipAnswer> Answers { get; set; } 
    }
}