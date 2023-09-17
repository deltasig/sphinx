using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class ScholarshipSubmission
{
    public Guid ScholarshipSubmissionId { get; set; }

    public int ScholarshipAppId { get; set; }

    public bool IsWinner { get; set; }

    public DateTime SubmittedOn { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string StudentNumber { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public string Address1 { get; set; }

    public string Address2 { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public int PostalCode { get; set; }

    public string Country { get; set; }

    public string HighSchool { get; set; }

    public int ActSatScore { get; set; }

    public double Gpa { get; set; }

    public string HearAboutScholarship { get; set; }

    public DateTime? CommitteeRespondedOn { get; set; }

    public string CommitteeResponse { get; set; }

    public virtual ICollection<ScholarshipAnswer> Answers { get; set; } = new List<ScholarshipAnswer>();

    public virtual ScholarshipApp Application { get; set; }
}
