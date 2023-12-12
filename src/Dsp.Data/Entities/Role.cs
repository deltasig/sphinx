using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Role : IdentityRole<int>
{
    public string Description { get; set; }

    public PositionType Type { get; set; }

    public bool IsExecutive { get; set; }

    public bool IsElected { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDisabled { get; set; }

    public bool IsPublic { get; set; }

    public bool CanBeRemoved { get; set; }

    public string Inquiries { get; set; }

    public virtual ICollection<UserRole> Users { get; set; } = new List<UserRole>();
}

public enum PositionType : int
{
    NonMember,
    Active,
    New,
    Alumni,
    Affiliate,
    Advisor
}