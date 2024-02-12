using System.Collections.Generic;

namespace Dsp.Data.Entities;

public partial class Position
{
    public virtual int Id { get; set; } = default!;

    public virtual string? Name { get; set; }

    public virtual string? NormalizedName { get; set; }

    public virtual string? ConcurrencyStamp { get; set; }

    public string Description { get; set; }

    public PositionType Type { get; set; }

    public bool IsExecutive { get; set; }

    public bool IsElected { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsDisabled { get; set; }

    public bool IsPublic { get; set; }

    public bool CanBeRemoved { get; set; }

    public string Inquiries { get; set; }

    public virtual ICollection<MemberPosition> Users { get; set; } = new List<MemberPosition>();
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