namespace Dsp.WebCore.Areas.Members.Models;

public class RosterFilterModel
{
    public int? sem { get; set; }
    public string sort { get; set; }
    public string order { get; set; }
    public string s { get; set; }

    public RosterFilterModel()
    {
        sem = null;
        sort = "last-name";
        order = "asc";
        s = string.Empty;
    }
    public RosterFilterModel(RosterFilterModel original)
    {
        sem = original.sem;
        sort = original.sort;
        order = original.order;
        s = original.s;
    }
}