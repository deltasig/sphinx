using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class IncidentReport
{
    public int Id { get; set; }

    public DateTime DateTimeSubmitted { get; set; }

    public int UserId { get; set; }

    public DateTime DateTimeOfIncident { get; set; }

    public string PolicyBroken { get; set; }

    public string Description { get; set; }

    public string OfficialReport { get; set; }

    public string InvestigationNotes { get; set; }

    public bool ShareIdentity { get; set; }

    public virtual Member User { get; set; }
}
